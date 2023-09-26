using DCL.Helpers;
using DCLServices.MapRendererV2;
using System;

namespace DCL
{
    public class NavMapChunksLayersController : IDisposable
    {
        private readonly NavMapChunksLayersView view;
        private readonly IMapRenderer mapRender;

        public NavMapChunksLayersController(NavMapChunksLayersView view)
        {
            this.view = view;

            this.view.ParcelsButtonClicked += EnableParcelsViewMode;
            this.view.SatelliteButtonClicked += EnableSatelliteViewMode;

            mapRender = Environment.i.serviceLocator.Get<IMapRenderer>();

            view.SetState(satelliteViewActive: true);
        }

        public void Dispose()
        {
            view.ParcelsButtonClicked -= EnableParcelsViewMode;
            view.SatelliteButtonClicked -= EnableSatelliteViewMode;

            Utils.SafeDestroy(view.gameObject);
        }

        private void EnableSatelliteViewMode()
        {
            view.SetState(satelliteViewActive: true);
            mapRender.SetSatelliteViewMode(true);
        }

        private void EnableParcelsViewMode()
        {
            view.SetState(satelliteViewActive: false);
            mapRender.SetSatelliteViewMode(false);
        }
    }
}
