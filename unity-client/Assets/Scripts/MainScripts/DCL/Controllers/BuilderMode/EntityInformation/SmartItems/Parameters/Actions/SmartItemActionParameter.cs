using DCL;
using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemActionParameter : SmartItemUIParameterAdapter, IEntityListHandler
{
    public ActionsListView actionsListView;
    public Button addActionBtn;

    List<DCLBuilderInWorldEntity> alreadyFilterList;

    private void Start()
    {
        addActionBtn.onClick.AddListener(CreateEventAction);
        actionsListView.OnActionableRemove += RemoveActionable;
    }

    private void RemoveActionable(SmartItemActionable actionable)
    {
        var actionsGeneric = GetParameterValue();
        if (actionsGeneric == null || !(actionsGeneric is List<SmartItemActionable>))
            return;

        SmartItemActionable actionableToRemove = null;
        List<SmartItemActionable> actions = (List<SmartItemActionable>)actionsGeneric;
        foreach(SmartItemActionable actionableItem in actions)
        {
            if (actionable.actionableId == actionableItem.actionableId)
                actionableToRemove = actionableItem;
        }
        actions.Remove(actionableToRemove);

        SetParameterValue(actions);
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        this.alreadyFilterList = BuilderInWorldUtils.FilterEntitiesBySmartItemComponentAndActions(entitiesList); 
    }

    public override void SetInfo()
    {
        base.SetInfo();

        KEY_NAME = currentParameter.id;

        var actionsGeneric = GetParameterValue();
        if (actionsGeneric == null || !(actionsGeneric is List<SmartItemActionable>))
            return;
        List<SmartItemActionable> actions = (List<SmartItemActionable>)actionsGeneric;

        foreach (SmartItemActionable smartItemAction in actions)
        {
            AddEventAction(smartItemAction);
        }
    }

    public void AddEventAction(SmartItemActionable action)
    {
        if (alreadyFilterList.Count <= 0)
            return;

        SmartItemActionEvent actionEvent = new SmartItemActionEvent();
        actionEvent.entityList = alreadyFilterList;
        actionEvent.smartItemActionable = action;

        if (currentValues.ContainsKey(action.actionableId))
            actionEvent.values = (Dictionary<object, object>) currentValues[action.actionableId];
        else
            actionEvent.values = new Dictionary<object, object>();

        actionsListView.AddActionEventAdapter(actionEvent);     
    }

    public void CreateEventAction()
    {
        var actionsGeneric = GetParameterValue();

        List<SmartItemActionable> actions;

        if (actionsGeneric != null && (actionsGeneric is List<SmartItemActionable>))
            actions = (List<SmartItemActionable>)actionsGeneric;
        else
            actions = new List<SmartItemActionable>();

        SmartItemActionable action = new SmartItemActionable();
        action.actionableId = Guid.NewGuid().ToString();
        actions.Add(action);     
        AddEventAction(action);
        SetParameterValue(actions);
    }
}
