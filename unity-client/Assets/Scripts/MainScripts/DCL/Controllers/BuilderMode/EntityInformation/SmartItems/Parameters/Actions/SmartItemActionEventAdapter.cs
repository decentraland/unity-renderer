using DCL;
using DCL.Components;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemActionEventAdapter : MonoBehaviour
{
    public TMP_Dropdown entityDropDown;
    public TMP_Dropdown actionDropDown;
    public TMP_Dropdown optionsDropDown;
    public SmartItemListView smartItemListView;

    public System.Action<SmartItemActionEventAdapter> OnActionableRemove;

    private SmartItemActionEvent actionEvent;

    private DCLBuilderInWorldEntity selectedEntity;
    private List<DCLBuilderInWorldEntity> filteredList = new List<DCLBuilderInWorldEntity>();

    private Dictionary<DCLBuilderInWorldEntity, Sprite> entitySpriteDict = new Dictionary<DCLBuilderInWorldEntity, Sprite>();
    private Dictionary<string, DCLBuilderInWorldEntity> entityPromiseKeeperDict = new Dictionary<string, DCLBuilderInWorldEntity>();

    private void Start()
    {
        entityDropDown.onValueChanged.AddListener(SelectedEntity);
        actionDropDown.onValueChanged.AddListener(GenerateParametersFromIndex);
        optionsDropDown.onValueChanged.AddListener(OptionSelected);
        optionsDropDown.SetValueWithoutNotify(-1);
    }

    private void OptionSelected(int index)
    {
        switch (index)
        {
            case 0:
                ResetActionable();
                break;
            case 1:
                RemoveActionable();
                break;
        }

        optionsDropDown.SetValueWithoutNotify(1);
    }

    public SmartItemActionEvent GetContent() { return actionEvent; }

    public void RemoveActionable()
    {
        OnActionableRemove?.Invoke(this);
        Destroy(gameObject);
    }

    public void ResetActionable()
    {
        actionEvent.smartItemActionable.values.Clear();
        SetContent(actionEvent);
    }

    public void SetContent(SmartItemActionEvent actionEvent)
    {
        this.actionEvent = actionEvent;
        filteredList = BuilderInWorldUtils.FilterEntitiesBySmartItemComponentAndActions(actionEvent.entityList);

        GenerateEntityDropdownContent();
        foreach (DCLBuilderInWorldEntity entity in filteredList)
        {
            GetThumbnail(entity);
        }

        SelectedEntity(entityDropDown.value);
    }

    private void SelectedEntity(int number)
    {
        if (!filteredList[number].rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent component))
        {
            return;
        }

        actionEvent.smartItemActionable.entityId = filteredList[number].rootEntity.entityId;
        selectedEntity = filteredList[number];
        GenerateActionDropdownContent(filteredList[number].GetSmartItemActions());

        GenerateParametersFromSelectedOption();
    }

    private void GenerateParametersFromSelectedOption() { GenerateParametersFromIndex(actionDropDown.value); }

    private void GenerateParametersFromIndex(int index)
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

        actionEvent.smartItemActionable.actionId = selectedAction.id;
        smartItemListView.SetEntityList(actionEvent.entityList);
        smartItemListView.SetSmartItemParameters(selectedAction.parameters, actionEvent.smartItemActionable.values);
    }

    private void GenerateActionDropdownContent(SmartItemAction[] actions)
    {
        actionDropDown.ClearOptions();

        actionDropDown.options = new List<TMP_Dropdown.OptionData>();

        List<string> optionsLabelList = new List<string>();
        int index = 0;
        int indexToUse = 0;

        foreach (SmartItemAction action in actions)
        {
            optionsLabelList.Add(action.label);
            if (!string.IsNullOrEmpty(actionEvent.smartItemActionable.actionId) &&
                action.id == actionEvent.smartItemActionable.actionId)
                indexToUse = index;

            index++;
        }

        actionDropDown.AddOptions(optionsLabelList);
        actionDropDown.SetValueWithoutNotify(indexToUse);
    }

    private void GenerateEntityDropdownContent()
    {
        entityDropDown.ClearOptions();

        entityDropDown.options = new List<TMP_Dropdown.OptionData>();

        List<TMP_Dropdown.OptionData> optionsList = new List<TMP_Dropdown.OptionData>();

        int index = 0;
        int indexToUse = 0;

        foreach (DCLBuilderInWorldEntity entity in filteredList)
        {
            var item = new TMP_Dropdown.OptionData();
            item.text = entity.GetDescriptiveName();
            if (entitySpriteDict.ContainsKey(entity))
                item.image = entitySpriteDict[entity];
            optionsList.Add(item);

            if (!string.IsNullOrEmpty(actionEvent.smartItemActionable.entityId) &&
                entity.rootEntity.entityId == actionEvent.smartItemActionable.entityId)
                indexToUse = index;

            index++;
        }

        entityDropDown.AddOptions(optionsList);
        entityDropDown.SetValueWithoutNotify(indexToUse);
    }

    private void GetThumbnail(DCLBuilderInWorldEntity entity)
    {
        var url = entity.GetCatalogItemAssociated()?.thumbnailURL;

        if (string.IsNullOrEmpty(url))
            return;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise = new AssetPromise_Texture(url);

        string promiseId = newLoadedThumbnailPromise.GetId().ToString();
        if (!entityPromiseKeeperDict.ContainsKey(promiseId))
            entityPromiseKeeperDict.Add(promiseId, entity);
        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        if (!entityPromiseKeeperDict.ContainsKey(texture.id.ToString()))
            return;
        Sprite spriteToUse = Sprite.Create(texture.texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        entitySpriteDict.Add(entityPromiseKeeperDict[texture.id.ToString()], spriteToUse);
        GenerateEntityDropdownContent();
    }
}