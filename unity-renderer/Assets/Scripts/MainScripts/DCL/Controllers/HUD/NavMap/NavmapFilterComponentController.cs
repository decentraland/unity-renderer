using DCL;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.MapLayers;
using System;

public class NavmapFilterComponentController : IDisposable
{
    private Service<IMapRenderer> mapRenderer;
    private INavmapFilterComponentView view;

    public NavmapFilterComponentController(INavmapFilterComponentView view)
    {
        this.view = view;

        view.OnFilterChanged += ToggleLayer;
    }

    private void ToggleLayer(MapLayer layerName, bool isActive) =>
        mapRenderer.Ref.ToggleLayer(layerName, isActive);

    public void Dispose()
    {
        view.OnFilterChanged += ToggleLayer;
    }
}
