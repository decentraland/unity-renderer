using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class BuildInWorldCompleteAction
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

    List<BuilderInWorldEntityAction> entityApplied = new List<BuilderInWorldEntityAction>();
    
    public void Redo()
    {
        foreach(BuilderInWorldEntityAction action in entityApplied)
        {
            ApplyValue(action.entityId,action.newValue, false);
        }
        isDone = true;
    }

    public void Undo()
    {
        foreach (BuilderInWorldEntityAction action in entityApplied)
        {
            ApplyValue(action.entityId, action.oldValue, true);
        }

        isDone = false;

    }

    void ApplyValue(string entityToApply, object value, bool isUndo)
    {
        OnApplyValue?.Invoke(entityToApply, value, actionType, isUndo);
    }

    public void CreateChangeFloorAction(SceneObject oldFloor,SceneObject newFloor)
    {
        BuilderInWorldEntityAction action = new BuilderInWorldEntityAction(JsonConvert.SerializeObject(oldFloor), JsonConvert.SerializeObject(newFloor));
        List<BuilderInWorldEntityAction> list = new List<BuilderInWorldEntityAction>();
        list.Add(action);
        CreateAction(list, ActionType.CHANGE_FLOOR);
    }

    public void CreateActionType(BuilderInWorldEntityAction action, ActionType type)
    {
        List<BuilderInWorldEntityAction> list = new List<BuilderInWorldEntityAction>();
        list.Add(action);
        CreateAction(list, type);
    }

    public void CreateActionType(List<BuilderInWorldEntityAction> entitiesActions, ActionType type)
    {
        CreateAction(entitiesActions, type);
    }

    void CreateAction(List<BuilderInWorldEntityAction> entitiesActions,ActionType type)
    {
        actionType = type;
        entityApplied = entitiesActions;
        isDone = true;
    }
}
