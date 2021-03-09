using UnityEngine;

internal interface IMapDataView
{
    Vector2Int baseCoord { get; }
    string name { get; }
    string creator { get; }
    string description { get; }
    void SetMinimapSceneInfo(HotScenesController.HotSceneInfo sceneInfo);
    bool ContainCoords(Vector2Int coords);
}
