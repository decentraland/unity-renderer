using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemEntityParameter : SmartItemUIParameterAdapter, IEntityListHandler
{
    public TMP_Dropdown dropDown;

    List<DCLBuilderInWorldEntity> entitiesList;

    private void Start()
    {
        dropDown.onValueChanged.AddListener(OnValueChange);
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        this.entitiesList = entitiesList;
    }

    public override void SetInfo()
    {
        base.SetInfo();

        GenerateDropdownContent();
    }

    void GenerateDropdownContent()
    {
        dropDown.ClearOptions();

        dropDown.options = new List<TMP_Dropdown.OptionData>();

        var item = new TMP_Dropdown.OptionData();
     

        List<string> optionsLabelList = new List<string>();
        foreach (DCLBuilderInWorldEntity entity in entitiesList)
        {
            optionsLabelList.Add(entity.GetDescriptiveName());
        }

        dropDown.AddOptions(optionsLabelList);


        string value = (string)GetParameterValue();

        for (int i = 0; i < entitiesList.Count; i++)
        {
            if (entitiesList[i].rootEntity.entityId == value)
                dropDown.SetValueWithoutNotify(i);
        }
    }

    private void GetThumbnail(DCLBuilderInWorldEntity entity)
    {
        var url = entity.GetCatalogItemAssociated()?.thumbnailURL;

        if (string.IsNullOrEmpty(url))
            return;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise = new AssetPromise_Texture(url);


        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        //TODO: Implement the Image of the entity for the dropdown
    }

    private void OnValueChange(int currentIndex)
    {
        foreach (DCLBuilderInWorldEntity entity in entitiesList)
        {
            if (entity.GetDescriptiveName() == dropDown.options[currentIndex].text)
                SetParameterValue(entity.rootEntity.entityId);
        }
    }
}
