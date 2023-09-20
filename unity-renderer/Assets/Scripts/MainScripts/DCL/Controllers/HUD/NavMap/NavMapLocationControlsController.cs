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
        private const float TRANSLATION_DURATION = 0.5f;
        private readonly NavMapLocationControlsView view;
        private readonly NavmapZoomViewController navmapZoomViewController;
        private readonly NavmapToastViewController toastViewController;

        private readonly MapRenderImage.ParcelClickData homeParcel = new ()
        {
            Parcel = DataStore.i.HUDs.homePoint.Get(),
            WorldPosition = new Vector2(Screen.width / 2f, Screen.height / 2f),
        };
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
            toastViewController.CloseCurrentToast();

            mapCamera.TranslateTo(
                coordinates: DataStore.i.HUDs.homePoint.Get(),
                zoom: navmapZoomViewController.ResetZoomToMidValue(),
                duration: TRANSLATION_DURATION,
                onComplete: () => toastViewController.ShowPlaceToast(homeParcel, showUntilClick: true));
        }

        private void CenterToPlayerLocation()
        {
            toastViewController.CloseCurrentToast();

            mapCamera.TranslateTo(
                coordinates: Utils.WorldToGridPosition(DataStore.i.player.playerWorldPosition.Get()),
                zoom: navmapZoomViewController.ResetZoomToMidValue(),
                duration: TRANSLATION_DURATION);
        }
    }
}
