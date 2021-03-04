using DCL.Configuration;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogItem 
{
    public enum ItemType
    {
        SCENE_OBJECT = 0,
        NFT_OBJECT = 1
    }


    public string id;
    public string asset_pack_id;
    public string name;
    public string model;
    public string thumbnail;
    public List<string> tags;
    public ItemType itemType;

    public string category;
    public Dictionary<string, string> contents;

    public SceneObject.ObjectMetrics metrics;

    public bool isFavorite = false;
    public bool alreadyCreated = false;

    string baseUrl;

    public CatalogItem(SceneObject sceneObject)
    {
        id = sceneObject.id;
        asset_pack_id = sceneObject.asset_pack_id;
        name = sceneObject.name;
        model = sceneObject.model;
        thumbnail = sceneObject.thumbnail;
        tags = sceneObject.tags;

        category = sceneObject.category;
        contents = sceneObject.contents;

        metrics = sceneObject.metrics;
        baseUrl = sceneObject.GetBaseURL();

        itemType = ItemType.SCENE_OBJECT;
    }

    public CatalogItem(NFTInfo nFTInfo)
    {
        asset_pack_id = BuilderInWorldSettings.ASSETS_COLLECTIBLES;
        id = nFTInfo.assetContract.address;
        thumbnail = nFTInfo.thumbnailUrl;
        SetBaseURL(nFTInfo.originalImageUrl);
        name = nFTInfo.name;
        category = nFTInfo.assetContract.name;
        model = BuilderInWorldSettings.COLLECTIBLE_MODEL_PROTOCOL+ nFTInfo.assetContract.address + "/" + nFTInfo.tokenId;
        tags = new List<string>();
        contents = new Dictionary<string, string>();
        metrics = new SceneObject.ObjectMetrics();
    }


    public string ComposeThumbnailUrl()
    {
        if (itemType == ItemType.SCENE_OBJECT)
            return baseUrl + thumbnail;
        else
            return thumbnail;
    }

    public void SetBaseURL(string newUrl)
    {
        baseUrl = newUrl;
    }

    public void SetBaseUrl(string newUrl)
    {
        baseUrl = newUrl;
    }
    
}
