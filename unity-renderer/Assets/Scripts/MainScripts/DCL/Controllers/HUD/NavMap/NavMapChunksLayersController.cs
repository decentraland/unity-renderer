using DCL.Helpers;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace DCL
{
    public class NavMapChunksLayersController : IDisposable
    {
        private const string GENESIS_CITY_URL = "https://genesis.city/";
        private readonly NavMapChunksLayersView view;
        private readonly IMapRenderer mapRender;

        public NavMapChunksLayersController(NavMapChunksLayersView view)
        {
            this.view = view;

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
        }

        private void EnableParcelsViewMode()
        {
            view.SetState(satelliteViewActive: false);

            mapRender.SetSharedLayer(MapLayer.ParcelsAtlas, true);
            mapRender.SetSharedLayer(MapLayer.SatelliteAtlas, false);
        }

        private static void OpenGenesisCityLink() =>
            Application.OpenURL(GENESIS_CITY_URL);
    }
}
