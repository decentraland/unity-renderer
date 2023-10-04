using DCL.Helpers;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.MapLayers;
using ExploreV2Analytics;
using System;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace DCL
{
    public class NavMapChunksLayersController : IDisposable
    {
        private const string GENESIS_CITY_URL = "https://genesis.city/";
        private readonly NavMapChunksLayersView view;
        private readonly IExploreV2Analytics exploreV2Analytics;
        private readonly IMapRenderer mapRender;

        public NavMapChunksLayersController(NavMapChunksLayersView view, IExploreV2Analytics exploreV2Analytics)
        {
            this.view = view;
            this.exploreV2Analytics = exploreV2Analytics;

            this.view.ParcelsButtonClicked += EnableParcelsViewMode;
            this.view.SatelliteButtonClicked += EnableSatelliteViewMode;
            this.view.HyperLinkClicked += OpenGenesisCityLink;

            mapRender = Environment.i.serviceLocator.Get<IMapRenderer>();
        }

        public void Dispose()
        {
            view.ParcelsButtonClicked -= EnableParcelsViewMode;
            view.SatelliteButtonClicked -= EnableSatelliteViewMode;
            view.HyperLinkClicked -= OpenGenesisCityLink;

            Utils.SafeDestroy(view.gameObject);
        }

        private void EnableSatelliteViewMode()
        {
            view.SetState(satelliteViewActive: true);

            mapRender.SetSharedLayer(MapLayer.SatelliteAtlas, true);
            mapRender.SetSharedLayer(MapLayer.ParcelsAtlas, false);

            exploreV2Analytics.SendToggleMapLayer(MapLayer.SatelliteAtlas.ToString(), true);
        }

        private void EnableParcelsViewMode()
        {
            view.SetState(satelliteViewActive: false);

            mapRender.SetSharedLayer(MapLayer.ParcelsAtlas, true);
            mapRender.SetSharedLayer(MapLayer.SatelliteAtlas, false);

            exploreV2Analytics.SendToggleMapLayer(MapLayer.ParcelsAtlas.ToString(), true);
        }

        private void OpenGenesisCityLink()
        {
            exploreV2Analytics.SendOpenGenesisCityUrl();
            Application.OpenURL(GENESIS_CITY_URL);
        }
    }
}
