using DCL.Helpers;
using System;
using System.Linq;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

public class MapInfoHandler : IMapDataView
{
    public Vector2Int baseCoord { private set; get; }
    public string name { private set; get; }
    public string creator { private set; get; }
    public string description { private set; get; }

    public Vector2Int[] parcels { private set; get; }

    public void SetMinimapSceneInfo(IHotScenesController.PlaceInfo sceneInfo)
    {
        baseCoord = Utils.ConvertStringToVector(sceneInfo.base_position);
        name = sceneInfo.title;
        creator = sceneInfo.owner;
        description = sceneInfo.description;
        parcels = sceneInfo.Positions;
    }

    public bool ContainCoords(Vector2Int coords)
    {
        if (parcels == null)
            return false;
        return parcels.Contains(coords);
    }
}
