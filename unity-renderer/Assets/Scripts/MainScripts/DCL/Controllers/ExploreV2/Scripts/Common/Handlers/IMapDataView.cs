using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

public interface IMapDataView
{
    Vector2Int baseCoord { get; }
    string name { get; }
    string creator { get; }
    string description { get; }
    void SetMinimapSceneInfo(IHotScenesController.PlaceInfo sceneInfo);
    bool ContainCoords(Vector2Int coords);
}
