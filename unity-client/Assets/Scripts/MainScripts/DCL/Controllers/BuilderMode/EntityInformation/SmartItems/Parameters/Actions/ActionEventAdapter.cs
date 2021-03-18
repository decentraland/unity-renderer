using DCL;
using DCL.Components;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionEventAdapter : MonoBehaviour
{
    public TMP_Dropdown entityDropDown;
    public TMP_Dropdown actionDropDown;
    public Button addActionBtn;
    public SmartItemListView smartItemListView;

    List<DCLBuilderInWorldEntity> entityList;

    SmartItemComponent selectedComponent;
    DCLBuilderInWorldEntity selectedEntity;
    List<DCLBuilderInWorldEntity> filteredList = new List<DCLBuilderInWorldEntity>();

    private void Start()
    {
        entityDropDown.onValueChanged.AddListener(SelectedEntity);
        actionDropDown.onValueChanged.AddListener(GenerateParametersFromIndex);
    }

    public void SetContent(List<DCLBuilderInWorldEntity> entityList)
    {
        this.entityList = entityList;
        filteredList = BuilderInWorldUtils.FilterEntitiesBySmartItemComponentAndActions(entityList);

        GenerateEntityDropdownContent();
        SelectedEntity(0);
    }

    void SelectedEntity(int number)
    {
        if (!filteredList[number].rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent component))
        {
            return;
        }

        selectedEntity = filteredList[number];
        selectedComponent = (SmartItemComponent) component;
        GenerateActionDropdownContent(filteredList[number].GetSmartItemActions());

        GenerateParametersFromSelectedOption();
    }

    void GenerateParametersFromSelectedOption()
    {
        GenerateParametersFromIndex(actionDropDown.value);
    }

    void GenerateParametersFromIndex(int index)
    {
        string label = actionDropDown.options[index].text;

        SmartItemAction selectedAction = null;
        foreach (SmartItemAction action in selectedEntity.GetSmartItemActions())
        {
            if (action.label == label)
            {
                selectedAction = action;
                break;
            }
        }

        smartItemListView.SetEntityList(entityList);
        smartItemListView.SetSmartItemParameters(selectedAction.parameters, selectedComponent.GetValues());
    }

    void GenerateActionDropdownContent(SmartItemAction[] actions)
    {
        actionDropDown.ClearOptions();

        actionDropDown.options = new List<TMP_Dropdown.OptionData>();


        List<string> optionsLabelList = new List<string>();
        foreach (SmartItemAction action in actions)
        {
            optionsLabelList.Add(action.label);
        }

        actionDropDown.AddOptions(optionsLabelList);
    }

    void GenerateEntityDropdownContent()
    {
        entityDropDown.ClearOptions();

        entityDropDown.options = new List<TMP_Dropdown.OptionData>();

        List<string> optionsLabelList = new List<string>();
        foreach (DCLBuilderInWorldEntity entity in filteredList)
        {
            optionsLabelList.Add(entity.GetDescriptiveName());
        }

        entityDropDown.AddOptions(optionsLabelList);
    }
}