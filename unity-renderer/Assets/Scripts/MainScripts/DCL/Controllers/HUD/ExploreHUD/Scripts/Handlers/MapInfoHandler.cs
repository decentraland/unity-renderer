using System;
using System.Linq;
using UnityEngine;

internal class MapInfoHandler : IMapDataView
{
    public Vector2Int baseCoord { private set; get; }
    public string name { private set; get; }
    public string creator { private set; get; }
    public string description { private set; get; }

    public Vector2Int[] parcels { private set; get; }

    public void SetMinimapSceneInfo(HotScenesController.HotSceneInfo sceneInfo)
    {
        baseCoord = sceneInfo.baseCoords;
        name = sceneInfo.name;
        creator = sceneInfo.creator;
        description = sceneInfo.description;
        parcels = sceneInfo.parcels;
    }

    public bool ContainCoords(Vector2Int coords)
    {
        if (parcels == null) return false;
        return parcels.Contains(coords);
    }
}
