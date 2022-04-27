using DCL.Components;
using DCL.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneObject
{
    [System.Serializable]
    public class ObjectMetrics
    {
        public int meshes;
        public int bodies;
        public int materials;
        public int textures;
        public int triangles;
        public int entities;
    }

    public string id;
    
    // This legacyId is not used for anything, but it is needed for the current builder, we can safely delete it when the web builder is no longer compatible
    public string legacyId = "";
    public string asset_pack_id;
    public string name;
    public string model;
    public string thumbnail;
    public List<string> tags;

    public string category;
    public string titleToShow;
    public Dictionary<string, string> contents;

    public string created_at;
    public string updated_at;

    public ObjectMetrics metrics;
    public SmartItemParameter[] parameters;
    public SmartItemAction[] actions;
    public string script;
    public bool isFavorite = false;

    public string GetComposedThumbnailUrl()
    {
        //NOTE: This is a workaround since the builder sometimes send the thumbnail composed and sometimes it doesn't
        //This way we ensure that the base url is only 1 time
        string urlBase = BIWUrlUtils.GetUrlSceneObjectContent();
        if (thumbnail != null)
            urlBase = urlBase + thumbnail.Replace(urlBase, "");
        return urlBase;
    }

    public string GetBaseURL() { return BIWUrlUtils.GetUrlSceneObjectContent(); }

    public bool IsSmartItem() { return !string.IsNullOrEmpty(script); }
}