using DCL.Components;
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
        NFT = 1,
        SMART_ITEM = 2
    }

    public string id;
    public ItemType itemType;

    public string name;
    public string model;

    public List<string> tags;
    public string category;
    public string categoryName;
    public Dictionary<string, string> contents;


    public SceneObject.ObjectMetrics metrics;
    public SmartItemParameter[] parameters;
    public SmartItemAction[] actions;
    public bool isVoxel = false;
    public string thumbnailURL;

    private bool isFavorite = false;

    public string GetThumbnailUrl() => thumbnailURL;

    public void SetFavorite(bool isFavorite) => this.isFavorite = isFavorite;

    public bool IsFavorite() => isFavorite;

    public bool IsNFT() => itemType == ItemType.NFT;

    public bool IsSmartItem() => itemType == ItemType.SMART_ITEM;

    public bool IsVoxel() => isVoxel;

    public bool HasActions()
    {
        if (actions.Length > 0)
            return true;
        
        return false;
    }
}
