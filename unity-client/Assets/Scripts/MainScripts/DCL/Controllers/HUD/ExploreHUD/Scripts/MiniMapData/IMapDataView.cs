using UnityEngine;

internal interface IMapDataView
{
    void SetMinimapSceneInfo(MinimapMetadata.MinimapSceneInfo info);
    MinimapMetadata.MinimapSceneInfo GetMinimapSceneInfo();
    bool HasMinimapSceneInfo();
    void SetBaseCoord(Vector2Int coords);
    Vector2Int GetBaseCoord();
    bool ContainCoords(Vector2Int coords);
}
