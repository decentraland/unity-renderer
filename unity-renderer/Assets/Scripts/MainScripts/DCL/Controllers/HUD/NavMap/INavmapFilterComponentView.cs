using DCLServices.MapRendererV2.MapLayers;
using System;

public interface INavmapFilterComponentView
{
    public event Action<MapLayer, bool> OnFilterChanged;
    event Action OnClickedDAO;
}
