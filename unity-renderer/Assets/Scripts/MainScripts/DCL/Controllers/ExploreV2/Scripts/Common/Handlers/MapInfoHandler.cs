using DCL.Helpers;
using System;
using System.Linq;
using UnityEngine;

public class MapInfoHandler : IMapDataView
{
    public Vector2Int baseCoord { private set; get; }
    public string name { private set; get; }
    public string creator { private set; get; }
    public string description { private set; get; }

    public Vector2Int[] parcels { private set; get; }

    public void SetMinimapSceneInfo(HotScenesController.PlaceInfo sceneInfo)
    {
        baseCoord = Utils.ConvertStringToVector(sceneInfo.base_position);
        name = sceneInfo.title;
        creator = sceneInfo.owner;
        description = sceneInfo.description;
        parcels = sceneInfo.positions;
    }

    public bool ContainCoords(Vector2Int coords)
    {
        if (parcels == null)
            return false;
        return parcels.Contains(coords);
    }
}
