using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeAction
{
    public enum ActionType
    {
        MOVE = 0,
        ROTATE = 1,
        SCALE = 2,
        CREATED = 3
    }

    public ActionType actionType;
    public bool isDone = true;
    public System.Action<DecentralandEntity, object,ActionType> OnApplyValue;
    List<BuildModeEntityAction> entitiyApplied = new List<BuildModeEntityAction>();
    
    public void ReDo()
    {
        foreach(BuildModeEntityAction action in entitiyApplied)
        {
            ApplyValue(action.entity,action.newValue);
        }
        isDone = true;
    }

    public void Undo()
    {
        foreach (BuildModeEntityAction action in entitiyApplied)
        {
            ApplyValue(action.entity, action.oldValue);
        }

        isDone = false;

    }

    void ApplyValue(DecentralandEntity entityToApply, object value)
    {
        OnApplyValue?.Invoke(entityToApply, value, actionType);
    }

    public void CreateActionType(BuildModeEntityAction action, ActionType type)
    {
        List<BuildModeEntityAction> list = new List<BuildModeEntityAction>();
        list.Add(action);
        CreateAction(list, type);
    }

    public void CreateActionType(List<BuildModeEntityAction> entitiesActions, ActionType type)
    {
        CreateAction(entitiesActions, type);
    }

    public void CreateActionTypeMove(List<BuildModeEntityAction> entitiesActions)
    {
        CreateAction(entitiesActions, ActionType.MOVE);
    }

    public void CreateActionTypeRotate(List<BuildModeEntityAction> entitiesActions)
    {
        CreateAction(entitiesActions, ActionType.ROTATE);
    }

    public void CreateActionTypeScale(List<BuildModeEntityAction> entitiesActions)
    {
        CreateAction(entitiesActions, ActionType.SCALE);
    }

    void CreateAction(List<BuildModeEntityAction> entitiesActions,ActionType type)
    {
        actionType = type;
        entitiyApplied = entitiesActions;
        isDone = true;
    }
}
