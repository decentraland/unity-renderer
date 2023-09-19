using DCL.Helpers;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.MapRendererV2.MapCameraController;
using System;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class NavMapLocationControlsController : IDisposable
    {
        private readonly NavMapLocationControlsView view;
        private readonly NavmapZoomViewController navmapZoomViewController;
        private readonly NavmapToastViewController toastViewController;
        private IMapCameraController mapCamera;

        private bool active;
        private CancellationTokenSource cts;

        public NavMapLocationControlsController(NavMapLocationControlsView view, NavmapZoomViewController navmapZoomViewController, NavmapToastViewController toastViewController)
        {
            this.view = view;
            this.navmapZoomViewController = navmapZoomViewController;
            this.toastViewController = toastViewController;
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        public void Activate(IMapCameraController mapCameraController)
        {
            if (active && mapCamera == mapCameraController)
                return;

            cts = new CancellationTokenSource();

            mapCamera = mapCameraController;

            view.homeButton.onClick.AddListener(FocusOnHomeLocation);
            view.centerToPlayerButton.onClick.AddListener(CenterToPlayerLocation);

            active = true;
        }

        public void Deactivate()
        {
            if (!active) return;

            cts.Cancel();
            cts.Dispose();
            cts = null;

            view.homeButton.onClick.RemoveListener(FocusOnHomeLocation);
            view.centerToPlayerButton.onClick.RemoveListener(CenterToPlayerLocation);

            active = false;
        }

        private void FocusOnHomeLocation()
        {
            mapCamera.SetPositionAndZoom(DataStore.i.HUDs.homePoint.Get(), navmapZoomViewController.ResetZoomToMidValue());

            var homeParcel = new MapRenderImage.ParcelClickData
                {
                    Parcel = DataStore.i.HUDs.homePoint.Get(),
                    WorldPosition = Vector2Int.zero,
                };

            toastViewController.ShowPlaceToast(homeParcel);
            // TODO: Open home card
        }

        private void CenterToPlayerLocation()
        {
            mapCamera.SetPositionAndZoom(Utils.WorldToGridPosition(DataStore.i.player.playerWorldPosition.Get()),
                navmapZoomViewController.ResetZoomToMidValue());
        }
    }
}
