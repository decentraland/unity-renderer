using DCL;
using DCL.Browser;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.MapLayers;
using ExploreV2Analytics;
using System;

public class NavmapFilterComponentController : IDisposable
{
    private const string DAO_LINK = "https://governance.decentraland.org/";

    private Service<IMapRenderer> mapRenderer;
    private INavmapFilterComponentView view;
    private IBrowserBridge browserBridge;
    private IExploreV2Analytics exploreV2Analytics;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly DataStore dataStore;

    public NavmapFilterComponentController(INavmapFilterComponentView view, IBrowserBridge browserBridge, IExploreV2Analytics exploreV2Analytics, IUserProfileBridge userProfileBridge, DataStore dataStore)
    {
        this.view = view;
        this.browserBridge = browserBridge;
        this.exploreV2Analytics = exploreV2Analytics;
        this.userProfileBridge = userProfileBridge;
        this.dataStore = dataStore;

        view.OnFilterChanged += ToggleLayer;
        view.OnClickedDAO += OpenDAOLink;
    }

    private void OpenDAOLink()
    {
        browserBridge.OpenUrl(DAO_LINK);
    }

    private void ToggleLayer(MapLayer layerName, bool isActive)
    {
        if (layerName.Equals(MapLayer.Friends) && userProfileBridge.GetOwn().isGuest)
        {
            dataStore.HUDs.connectWalletModalVisible.Set(true);
            return;
        }

        mapRenderer.Ref.SetSharedLayer(layerName, isActive);
        exploreV2Analytics.SendToggleMapLayer(layerName.ToString(), isActive);
    }

    public void Dispose()
    {
        view.OnFilterChanged -= ToggleLayer;
        view.OnClickedDAO -= OpenDAOLink;
    }
}
