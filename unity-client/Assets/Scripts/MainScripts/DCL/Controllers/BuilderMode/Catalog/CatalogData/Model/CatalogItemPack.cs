using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogItemPack 
{
    public string id;
    public string title;

    public List<CatalogItem> assets;

    string thumbnailURL;

    public string GetThumbnailUrl()
    {
        return thumbnailURL;
    }

    public void SetThumbnailULR(string url)
    {
        thumbnailURL = url;
    }
}
