using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class BIWCompleteAction : IBIWCompleteAction
{

    public IBIWCompleteAction.ActionType actionType;
    public bool isDone = true;

    public event IBIWCompleteAction.OnApplyValueDelegate OnApplyValue;

    List<BIWEntityAction> entityApplied = new List<BIWEntityAction>();

    public void Redo()
    {
        foreach (BIWEntityAction action in entityApplied)
        {
            ApplyValue(action.entityId, action.newValue, false);
        }
        isDone = true;
    }
    public bool IsDone() => isDone;

    public void Undo()
    {
        foreach (BIWEntityAction action in entityApplied)
        {
            ApplyValue(action.entityId, action.oldValue, true);
        }

        isDone = false;

    }

    private void ApplyValue(long entityToApply, object value, bool isUndo) { OnApplyValue?.Invoke(entityToApply, value, actionType, isUndo); }

    public void CreateChangeFloorAction(CatalogItem oldFloor, CatalogItem newFloor)
    {
        BIWEntityAction action = new BIWEntityAction(JsonConvert.SerializeObject(oldFloor), JsonConvert.SerializeObject(newFloor));
        List<BIWEntityAction> list = new List<BIWEntityAction>();
        list.Add(action);
        CreateAction(list, IBIWCompleteAction.ActionType.CHANGE_FLOOR);
    }

    public void CreateActionType(BIWEntityAction action, IBIWCompleteAction.ActionType type)
    {
        List<BIWEntityAction> list = new List<BIWEntityAction>();
        list.Add(action);
        CreateAction(list, type);
    }

    public void CreateActionType(List<BIWEntityAction> entitiesActions, IBIWCompleteAction.ActionType type) { CreateAction(entitiesActions, type); }

    void CreateAction(List<BIWEntityAction> entitiesActions, IBIWCompleteAction.ActionType type)
    {
        actionType = type;
        entityApplied = entitiesActions;
        isDone = true;
    }
}