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

            this.view.ParcelsButtonClicked += OnParcelsButtonClicked;
            this.view.SatelliteButtonClicked += OnSatelliteButtonClicked;

            mapRender = Environment.i.serviceLocator.Get<IMapRenderer>();

            view.SetState(satelliteViewActive: true);
        }

        public void Dispose()
        {
            view.ParcelsButtonClicked -= OnParcelsButtonClicked;
            view.SatelliteButtonClicked -= OnSatelliteButtonClicked;

            Utils.SafeDestroy(view.gameObject);
        }

        private void OnParcelsButtonClicked()
        {
            view.SetState(satelliteViewActive: false);
            mapRender.SetSatelliteViewMode(false);
        }

        private void OnSatelliteButtonClicked()
        {
            view.SetState(satelliteViewActive: true);
            mapRender.SetSatelliteViewMode(true);
        }
    }
}
