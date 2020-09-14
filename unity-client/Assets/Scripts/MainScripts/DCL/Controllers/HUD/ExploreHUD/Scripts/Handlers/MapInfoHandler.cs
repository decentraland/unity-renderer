using System;
using UnityEngine;

internal class MapInfoHandler : IMapDataView
{
    public MinimapMetadata.MinimapSceneInfo info { private set; get; }
    public Vector2Int baseCoord { private set; get; }
    public event Action<MinimapMetadata.MinimapSceneInfo> onInfoUpdate;

    public void SetMinimapSceneInfo(MinimapMetadata.MinimapSceneInfo info)
    {
        this.info = info;
        onInfoUpdate?.Invoke(info);
    }

    public MinimapMetadata.MinimapSceneInfo GetMinimapSceneInfo()
    {
        return info;
    }

    public bool HasMinimapSceneInfo()
    {
        return info != null;
    }

    public void SetBaseCoord(Vector2Int coords)
    {
        baseCoord = coords;
    }

    public Vector2Int GetBaseCoord()
    {
        return baseCoord;
    }

    public bool ContainCoords(Vector2Int coords)
    {
        if (info == null) return false;
        return info.parcels.Contains(coords);
    }

    public void Clear()
    {
        info = null;
    }
}
