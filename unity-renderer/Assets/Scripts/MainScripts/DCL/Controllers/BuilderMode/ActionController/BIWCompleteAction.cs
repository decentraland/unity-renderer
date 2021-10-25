using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class BIWCompleteAction : IBIWCompleteAction
{
    public enum ActionType
    {
        MOVE = 0,
        ROTATE = 1,
        SCALE = 2,
        CREATE = 3,
        DELETE = 4,
        CHANGE_FLOOR = 5
    }

    public ActionType actionType;
    public bool isDone = true;

    public delegate void OnApplyValueDelegate(string entityId, object value, ActionType actionType, bool isUndo);
    public event OnApplyValueDelegate OnApplyValue;

    List<BIWEntityAction> entityApplied = new List<BIWEntityAction>();

    public void Redo()
    {
        foreach (BIWEntityAction action in entityApplied)
        {
            ApplyValue(action.entityId, action.newValue, false);
        }
        isDone = true;
    }

    public void Undo()
    {
        foreach (BIWEntityAction action in entityApplied)
        {
            ApplyValue(action.entityId, action.oldValue, true);
        }

        isDone = false;

    }

    void ApplyValue(string entityToApply, object value, bool isUndo) { OnApplyValue?.Invoke(entityToApply, value, actionType, isUndo); }

    public void CreateChangeFloorAction(CatalogItem oldFloor, CatalogItem newFloor)
    {
        BIWEntityAction action = new BIWEntityAction(JsonConvert.SerializeObject(oldFloor), JsonConvert.SerializeObject(newFloor));
        List<BIWEntityAction> list = new List<BIWEntityAction>();
        list.Add(action);
        CreateAction(list, ActionType.CHANGE_FLOOR);
    }

    public void CreateActionType(BIWEntityAction action, ActionType type)
    {
        List<BIWEntityAction> list = new List<BIWEntityAction>();
        list.Add(action);
        CreateAction(list, type);
    }

    public void CreateActionType(List<BIWEntityAction> entitiesActions, ActionType type) { CreateAction(entitiesActions, type); }

    void CreateAction(List<BIWEntityAction> entitiesActions, ActionType type)
    {
        actionType = type;
        entityApplied = entitiesActions;
        isDone = true;
    }
}