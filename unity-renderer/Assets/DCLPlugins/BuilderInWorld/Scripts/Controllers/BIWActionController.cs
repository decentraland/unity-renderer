using System;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;
using static BIWCompleteAction;

public class BIWActionController : BIWController, IBIWActionController
{
    private static bool VERBOSE = false;

    public event System.Action OnRedo;
    public event System.Action OnUndo;

    private IBIWEntityHandler entityHandler;
    private IBIWFloorHandler floorHandler;

    private readonly List<IBIWCompleteAction> actionsMade = new List<IBIWCompleteAction>();

    private int currentUndoStepIndex = 0;
    private int currentRedoStepIndex = 0;

    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        entityHandler  = context.editorContext.entityHandler;
        floorHandler = context.editorContext.floorHandler;

        if ( context.editorContext.editorHUD == null)
            return;
        context.editorContext.editorHUD.OnUndoAction += TryToUndoAction;
        context.editorContext.editorHUD.OnRedoAction += TryToRedoAction;
    }

    public override void Dispose()
    {
        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnUndoAction -= TryToUndoAction;
            context.editorContext.editorHUD.OnRedoAction -= TryToRedoAction;
        }

        Clear();
    }

    public override void EnterEditMode(IParcelScene scene)
    {
        base.EnterEditMode(scene);
        actionsMade.Clear();

        CheckButtonsInteractability();
    }

    public void Clear()
    {
        actionsMade.Clear();
        currentUndoStepIndex = 0;
        currentRedoStepIndex = 0;
    }

    [ExcludeFromCodeCoverage]
    public void GoToAction(BIWCompleteAction action)
    {
        int index = actionsMade.IndexOf(action);
        int stepsAmount = currentUndoStepIndex - index;

        for (int i = 0; i <= Mathf.Abs(stepsAmount); i++)
        {
            if (stepsAmount > 0)
            {
                UndoCurrentAction();
                if (currentUndoStepIndex > 0)
                    currentUndoStepIndex--;
            }
            else
            {
                RedoCurrentAction();
                if (currentUndoStepIndex + 1 < actionsMade.Count)
                    currentUndoStepIndex++;
            }
        }
    }

    public void TryToRedoAction()
    {
        if (currentRedoStepIndex >= actionsMade.Count || currentRedoStepIndex < 0)
            return;

        RedoCurrentAction();

        if (currentRedoStepIndex + 1 < actionsMade.Count)
            currentRedoStepIndex++;

        if (currentUndoStepIndex < actionsMade.Count - 1)
            currentUndoStepIndex++;

        if (VERBOSE)
            Debug.Log("Redo:  Current actions " + actionsMade.Count + "   Current undo index " + currentUndoStepIndex + "   Current redo index " + currentRedoStepIndex);
    }

    public void TryToUndoAction()
    {
        if (currentUndoStepIndex < 0 ||
            actionsMade.Count <= 0 ||
            !actionsMade[0].IsDone())
            return;

        UndoCurrentAction();

        if (currentUndoStepIndex > 0)
        {
            currentUndoStepIndex--;
            if (currentRedoStepIndex < actionsMade.Count - 1 || currentRedoStepIndex - currentUndoStepIndex > 1)
                currentRedoStepIndex--;
        }
        else if (!actionsMade[currentUndoStepIndex].IsDone() && currentRedoStepIndex > 0)
        {
            currentRedoStepIndex--;
        }

        if (VERBOSE)
            Debug.Log("Undo:  Current actions " + actionsMade.Count + "   Current undo index " + currentUndoStepIndex + "   Current redo index " + currentRedoStepIndex);
    }

    public void CreateActionEntityDeleted(BIWEntity entity) { CreateActionEntityDeleted(new List<BIWEntity> { entity }); }

    public void CreateActionEntityDeleted(List<BIWEntity> entityList)
    {
        BIWCompleteAction buildAction = new BIWCompleteAction();
        List<BIWEntityAction> entityActionList = new List<BIWEntityAction>();

        foreach (BIWEntity entity in entityList)
        {
            BIWEntityAction biwEntityAction = new BIWEntityAction(entity.rootEntity.entityId, BIWUtils.ConvertEntityToJSON(entity.rootEntity), entity.rootEntity.entityId);
            entityActionList.Add(biwEntityAction);
        }

        buildAction.CreateActionType(entityActionList, IBIWCompleteAction.ActionType.DELETE);

        AddAction(buildAction);
    }

    public void CreateActionEntityCreated(IDCLEntity entity)
    {
        BIWEntityAction biwEntityAction = new BIWEntityAction(entity, entity.entityId, BIWUtils.ConvertEntityToJSON(entity));

        BIWCompleteAction buildAction = new BIWCompleteAction();
        buildAction.CreateActionType(biwEntityAction, IBIWCompleteAction.ActionType.CREATE);

        AddAction(buildAction);
    }

    public void AddAction(IBIWCompleteAction action)
    {
        if (currentRedoStepIndex < actionsMade.Count - 1)
            actionsMade.RemoveRange(currentRedoStepIndex, actionsMade.Count - currentRedoStepIndex);
        else if (actionsMade.Count > 0 && !actionsMade[currentRedoStepIndex].IsDone())
            actionsMade.RemoveAt(actionsMade.Count - 1);

        actionsMade.Add(action);

        currentUndoStepIndex = actionsMade.Count - 1;
        currentRedoStepIndex = actionsMade.Count - 1;


        if (VERBOSE)
            Debug.Log("Redo:  Current actions " + actionsMade.Count + "   Current undo index " + currentUndoStepIndex + "   Current redo index " + currentRedoStepIndex);
        action.OnApplyValue += ApplyAction;
        CheckButtonsInteractability();
    }

    void ApplyAction(string entityIdToApply, object value, IBIWCompleteAction.ActionType actionType, bool isUndo)
    {
        switch (actionType)
        {
            case IBIWCompleteAction.ActionType.MOVE:
                Vector3 convertedPosition = (Vector3) value;
                entityHandler.GetConvertedEntity(entityIdToApply).rootEntity.gameObject.transform.position = convertedPosition;
                break;

            case IBIWCompleteAction.ActionType.ROTATE:
                Vector3 convertedAngles = (Vector3) value;
                entityHandler.GetConvertedEntity(entityIdToApply).rootEntity.gameObject.transform.eulerAngles = convertedAngles;
                break;

            case IBIWCompleteAction.ActionType.SCALE:
                Vector3 convertedScale = (Vector3) value;
                IDCLEntity entityToApply = entityHandler.GetConvertedEntity(entityIdToApply).rootEntity;
                Transform parent = entityToApply.gameObject.transform.parent;

                entityToApply.gameObject.transform.localScale = new Vector3(convertedScale.x / parent.localScale.x, convertedScale.y / parent.localScale.y, convertedScale.z / parent.localScale.z);
                break;

            case IBIWCompleteAction.ActionType.CREATE:
                string entityString = (string) value;
                if (isUndo)
                    entityHandler.DeleteEntity(entityString);
                else
                    entityHandler.CreateEntityFromJSON(entityString);

                break;

            case IBIWCompleteAction.ActionType.DELETE:
                string deletedEntityString = (string) value;

                if (isUndo)
                    entityHandler.CreateEntityFromJSON(deletedEntityString);
                else
                    entityHandler.DeleteEntity(deletedEntityString);

                break;
            case IBIWCompleteAction.ActionType.CHANGE_FLOOR:
                string catalogItemToApply = (string) value;

                CatalogItem floorObject = JsonConvert.DeserializeObject<CatalogItem>(catalogItemToApply);
                entityHandler.DeleteFloorEntities();
                floorHandler.CreateFloor(floorObject);
                break;
        }
    }

    void RedoCurrentAction()
    {
        if (!actionsMade[currentRedoStepIndex].IsDone())
        {
            actionsMade[currentRedoStepIndex].Redo();
            OnRedo?.Invoke();

            CheckButtonsInteractability();
        }
    }

    void UndoCurrentAction()
    {
        if (actionsMade[currentUndoStepIndex].IsDone())
        {
            actionsMade[currentUndoStepIndex].Undo();
            OnUndo?.Invoke();

            CheckButtonsInteractability();
        }
    }

    void CheckButtonsInteractability()
    {
        if ( context.editorContext.editorHUD == null)
            return;

        bool canRedoAction = actionsMade.Count > 0 && !(currentRedoStepIndex == actionsMade.Count - 1 && actionsMade[actionsMade.Count - 1].IsDone());
        bool canUndoAction = actionsMade.Count > 0 && !(currentUndoStepIndex == 0 && !actionsMade[0].IsDone());

        context.editorContext.editorHUD.SetRedoButtonInteractable(canRedoAction);
        context.editorContext.editorHUD.SetUndoButtonInteractable(canUndoAction);
    }
}