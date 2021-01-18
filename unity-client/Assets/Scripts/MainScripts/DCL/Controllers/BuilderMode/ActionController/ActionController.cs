using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuildInWorldCompleteAction;

public class ActionController : MonoBehaviour
{
    public static bool VERBOSE = false;

    public BuilderInWorldController builderInWorldController;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;

    public System.Action OnUndo, OnRedo;


    List<BuildInWorldCompleteAction> actionsMade = new List<BuildInWorldCompleteAction>();

    int currentUndoStepIndex = 0;
    int currentRedoStepIndex = 0;

    public void ResetActionList()
    {
        actionsMade.Clear();
        currentUndoStepIndex = 0;
        currentRedoStepIndex = 0;
    }

    public void GoToAction(BuildInWorldCompleteAction action)
    {
        int index = actionsMade.IndexOf(action);
        int stepsAmount = currentUndoStepIndex - index;

        for(int i = 0; i <= Mathf.Abs(stepsAmount); i++)
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
        if (currentRedoStepIndex >= actionsMade.Count || currentRedoStepIndex < 0) return;

        RedoCurrentAction();

        if (currentRedoStepIndex + 1 < actionsMade.Count)
            currentRedoStepIndex++;

        if (currentUndoStepIndex < actionsMade.Count - 1) currentUndoStepIndex++;

        if(VERBOSE)
            Debug.Log("Redo:  Current actions " + actionsMade.Count + "   Current undo index " + currentUndoStepIndex + "   Current redo index " + currentRedoStepIndex);

    }

    public void TryToUndoAction()
    {
        if (currentUndoStepIndex < 0 ||
            actionsMade.Count <= 0 ||
            !actionsMade[0].isDone) return;

        UndoCurrentAction();

        if (currentUndoStepIndex > 0)
        {
            currentUndoStepIndex--;
            if (currentRedoStepIndex < actionsMade.Count - 1 || currentRedoStepIndex - currentUndoStepIndex > 1) currentRedoStepIndex--;
        }
        else if (!actionsMade[currentUndoStepIndex].isDone && currentRedoStepIndex > 0)
        {
            currentRedoStepIndex--;
        }
        if (VERBOSE)
            Debug.Log("Undo:  Current actions " + actionsMade.Count + "   Current undo index " + currentUndoStepIndex + "   Current redo index " + currentRedoStepIndex);

    }

    public void CreateActionEntityDeleted(List<DCLBuilderInWorldEntity> entityList)
    {
        BuildInWorldCompleteAction buildAction = new BuildInWorldCompleteAction();
        List<BuilderInWorldEntityAction> entityActionList = new List<BuilderInWorldEntityAction>();

        foreach (DCLBuilderInWorldEntity entity in entityList)
        {
            BuilderInWorldEntityAction builderInWorldEntityAction = new BuilderInWorldEntityAction(entity.rootEntity.entityId, BuilderInWorldUtils.ConvertEntityToJSON(entity.rootEntity), entity.rootEntity.entityId);
            entityActionList.Add(builderInWorldEntityAction);
        }

        buildAction.CreateActionType(entityActionList, BuildInWorldCompleteAction.ActionType.DELETE);

        AddAction(buildAction);
    }

    public void CreateActionEntityCreated(DecentralandEntity entity)
    {
        BuilderInWorldEntityAction builderInWorldEntityAction = new BuilderInWorldEntityAction(entity, entity.entityId, BuilderInWorldUtils.ConvertEntityToJSON(entity));

        BuildInWorldCompleteAction buildAction = new BuildInWorldCompleteAction();
        buildAction.CreateActionType(builderInWorldEntityAction, ActionType.CREATE);

        AddAction(buildAction);
    }

    public void AddAction(BuildInWorldCompleteAction action)
    {
        if (currentRedoStepIndex < actionsMade.Count - 1)
            actionsMade.RemoveRange(currentRedoStepIndex, actionsMade.Count - currentRedoStepIndex);
        else if (actionsMade.Count > 0 && !actionsMade[currentRedoStepIndex].isDone)
            actionsMade.RemoveAt(actionsMade.Count - 1);
        
        actionsMade.Add(action);
    
        currentUndoStepIndex = actionsMade.Count - 1;
        currentRedoStepIndex = actionsMade.Count - 1;


        if (VERBOSE)
            Debug.Log("Redo:  Current actions " + actionsMade.Count + "   Current undo index " + currentUndoStepIndex + "   Current redo index " + currentRedoStepIndex);
        action.OnApplyValue += ApplyAction;
    }

    void ApplyAction(string entityIdToApply, object value, ActionType actionType, bool isUndo)
    {
        switch (actionType)
        {
            case ActionType.MOVE:
                Vector3 convertedPosition = (Vector3)value;
                builderInWorldEntityHandler.GetEntity(entityIdToApply).rootEntity.gameObject.transform.position = convertedPosition;
                break;

            case ActionType.ROTATE:
                Vector3 convertedAngles = (Vector3)value;
                builderInWorldEntityHandler.GetEntity(entityIdToApply).rootEntity.gameObject.transform.eulerAngles = convertedAngles;
                break;

            case ActionType.SCALE:
                Vector3 convertedScale = (Vector3)value;
                DecentralandEntity entityToApply = builderInWorldEntityHandler.GetEntity(entityIdToApply).rootEntity;
                Transform parent = entityToApply.gameObject.transform.parent;

                entityToApply.gameObject.transform.localScale = new Vector3(convertedScale.x / parent.localScale.x, convertedScale.y / parent.localScale.y, convertedScale.z / parent.localScale.z);
                break;

            case ActionType.CREATE:
                string entityString = (string)value;
                if (isUndo)                
                    builderInWorldEntityHandler.DeleteEntity(entityString);                
                else               
                    builderInWorldEntityHandler.CreateEntityFromJSON(entityString);
                
                break;

            case ActionType.DELETE:
                string deletedEntityString = (string)value;

                if (isUndo)               
                    builderInWorldEntityHandler.CreateEntityFromJSON(deletedEntityString);                                
                else               
                    builderInWorldEntityHandler.DeleteEntity(deletedEntityString);
                
                break;
            case ActionType.CHANGE_FLOOR:
                string sceneObjectToApply = (string)value;
               
                SceneObject floorObject = JsonConvert.DeserializeObject<SceneObject>(sceneObjectToApply);
                builderInWorldEntityHandler.DeleteFloorEntities();
                builderInWorldController.CreateFloor(floorObject);
                break;
        }
    }

    void RedoCurrentAction()
    {
        if (!actionsMade[currentRedoStepIndex].isDone)
        {
            actionsMade[currentRedoStepIndex].Redo();
            OnRedo?.Invoke();        
        }  
    }

    void UndoCurrentAction()
    {
        if (actionsMade[currentUndoStepIndex].isDone)
        {
            actionsMade[currentUndoStepIndex].Undo();
            OnUndo?.Invoke();         
        }
    }

}
