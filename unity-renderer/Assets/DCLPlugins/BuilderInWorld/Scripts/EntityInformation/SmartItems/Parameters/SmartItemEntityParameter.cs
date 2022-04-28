using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemEntityParameter : SmartItemUIParameterAdapter, IEntityListHandler
{
    public TMP_Dropdown dropDown;

    private List<BIWEntity> entitiesList;

    private Dictionary<BIWEntity, Sprite> entitySpriteDict = new Dictionary<BIWEntity, Sprite>();
    private Dictionary<string, BIWEntity> entityPromiseKeeperDict = new Dictionary<string, BIWEntity>();

    private void Start() { dropDown.onValueChanged.AddListener(OnValueChange); }

    public void SetEntityList(List<BIWEntity> entitiesList) { this.entitiesList = entitiesList; }

    public override void SetInfo()
    {
        base.SetInfo();

        GenerateDropdownContent();
        foreach (BIWEntity entity in entitiesList)
        {
            GetThumbnail(entity);
        }
    }

    void GenerateDropdownContent()
    {
        dropDown.ClearOptions();

        dropDown.options = new List<TMP_Dropdown.OptionData>();

        List<TMP_Dropdown.OptionData> optionsList = new List<TMP_Dropdown.OptionData>();
        foreach (BIWEntity entity in entitiesList)
        {
            var item = new TMP_Dropdown.OptionData();
            item.text = entity.GetDescriptiveName();
            if (entitySpriteDict.ContainsKey(entity))
                item.image = entitySpriteDict[entity];
            optionsList.Add(item);
        }

        dropDown.AddOptions(optionsList);


        long value = (long) GetParameterValue();

        for (int i = 0; i < entitiesList.Count; i++)
        {
            if (entitiesList[i].rootEntity.entityId == value)
                dropDown.SetValueWithoutNotify(i);
        }
    }

    private void GetThumbnail(BIWEntity entity)
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
        newLoadedThumbnailPromise.OnFailEvent += (x, error) => { Debug.Log($"Error downloading: {url}, Exception: {error}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        if (!entityPromiseKeeperDict.ContainsKey(texture.id.ToString()))
            return;

        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite spriteToUse = Sprite.Create(texture.texture, new Rect(0, 0, texture.width, texture.height), pivot);
        entitySpriteDict.Add(entityPromiseKeeperDict[texture.id.ToString()], spriteToUse);
        GenerateDropdownContent();
    }

    private void OnValueChange(int currentIndex)
    {
        foreach (BIWEntity entity in entitiesList)
        {
            if (entity.GetDescriptiveName() == dropDown.options[currentIndex].text)
                SetParameterValue(entity.rootEntity.entityId);
        }
    }
}