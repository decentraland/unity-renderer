using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemEntityParameter : SmartItemUIParameterAdapter
{
    public TMP_Dropdown dropDown;

    const string parameterType = "entity";

    List<DCLBuilderInWorldEntity> entitiesList;

    public override void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        base.SetEntityList(entitiesList);

        this.entitiesList = entitiesList;
    }

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        if (parameter.type != parameterType)
            return;

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
    }

    private void GetThumbnail(DCLBuilderInWorldEntity entity)
    {
        var url = entity.GetSceneObjectAssociated()?.GetComposedThumbnailUrl();

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
}
