using System;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

public class MinimapMetadataController : MonoBehaviour
{
    private MinimapMetadata minimapMetadata => MinimapMetadata.GetMetadata();
    public static MinimapMetadataController i { get; private set; }

    [Serializable]
    private class MinimapSceneInfo
    {
        public string name;
        public int type;
        public List<Vector2Int> parcels;
    }

    public void Awake()
    {
        i = this;
        minimapMetadata.Clear();
    }

    public void UpdateMinimapSceneInformation(string scenesInfoJson)
    {
        var scenesInfo = Utils.ParseJsonArray<MinimapSceneInfo[]>(scenesInfoJson);
        foreach (var scene in scenesInfo)
        {
            foreach (var parcel in scene.parcels)
            {
                minimapMetadata.SetTile(parcel.x, parcel.y, new MinimapMetadata.Tile(scene.type, scene.name));
            }
        }
    }
}