using DCL;
using DCL.Browser;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.MapLayers;
using System;

public class NavmapFilterComponentController : IDisposable
{
    private const string DAO_LINK = "https://governance.decentraland.org/";

    private Service<IMapRenderer> mapRenderer;
    private INavmapFilterComponentView view;
    private IBrowserBridge browserBridge;

    public NavmapFilterComponentController(INavmapFilterComponentView view, IBrowserBridge browserBridge)
    {
        this.view = view;
        this.browserBridge = browserBridge;

        view.OnFilterChanged += ToggleLayer;
        view.OnClickedDAO += OpenDAOLink;
    }

    private void OpenDAOLink()
    {
        browserBridge.OpenUrl(DAO_LINK);
    }

    private void ToggleLayer(MapLayer layerName, bool isActive) =>
        mapRenderer.Ref.ToggleLayer(layerName, isActive);

    public void Dispose()
    {
        view.OnFilterChanged += ToggleLayer;
        view.OnClickedDAO -= OpenDAOLink;
    }
}
