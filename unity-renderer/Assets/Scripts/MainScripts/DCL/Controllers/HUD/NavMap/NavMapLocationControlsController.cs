using DCL.Helpers;
using DCL.Tasks;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.MapRendererV2.MapCameraController;
using ExploreV2Analytics;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL
{
    public class NavMapLocationControlsController : IDisposable
    {
        public const float TRANSLATION_DURATION = 0.5f;

        internal readonly BaseVariable<Vector2Int> homePoint;
        internal readonly BaseVariable<Vector3> playerPlayerWorldPosition;

        private readonly INavMapLocationControlsView view;
        private readonly IExploreV2Analytics exploreV2Analytics;
        private readonly INavmapZoomViewController navmapZoomViewController;
        private readonly INavmapToastViewController toastViewController;

        private IMapCameraController mapCamera;

        private bool active;
        private CancellationTokenSource cts;

        private MapRenderImage.ParcelClickData homeParcel => new()
        {
            Parcel = homePoint.Get(),
            WorldPosition = new Vector2(Screen.width / 2f, Screen.height / 2f),
        };

        public NavMapLocationControlsController(INavMapLocationControlsView view, IExploreV2Analytics exploreV2Analytics, INavmapZoomViewController navmapZoomViewController,
            INavmapToastViewController toastViewController, BaseVariable<Vector2Int> homePoint,
            BaseVariable<Vector3> playerPlayerWorldPosition)
        {
            this.view = view;
            this.exploreV2Analytics = exploreV2Analytics;
            this.navmapZoomViewController = navmapZoomViewController;
            this.toastViewController = toastViewController;
            this.homePoint = homePoint;
            this.playerPlayerWorldPosition = playerPlayerWorldPosition;
        }

        public void Dispose()
        {
            cts?.SafeCancelAndDispose();
            cts = null;
        }

        public void Activate(IMapCameraController mapCameraController)
        {
            if (active && mapCamera == mapCameraController)
                return;

            cts ??= new CancellationTokenSource();

            mapCamera = mapCameraController;

            view.HomeButtonClicked += FocusOnHomeLocation;
            view.CenterToPlayerButtonClicked += CenterToPlayerLocation;

            active = true;
        }

        public void Deactivate()
        {
            if (!active) return;

            cts?.SafeRestart();

            view.HomeButtonClicked -= FocusOnHomeLocation;
            view.CenterToPlayerButtonClicked -= CenterToPlayerLocation;

            active = false;
        }

        public void Hide()
        {
            Deactivate();
            view.Hide();
        }

        private void FocusOnHomeLocation()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            toastViewController.CloseCurrentToast();

            mapCamera.TranslateTo(
                coordinates: homePoint.Get(),
                duration: TRANSLATION_DURATION,
                onComplete: () => toastViewController.ShowPlaceToast(homeParcel, showUntilClick: true));

            exploreV2Analytics.SendCenterMapToHome();
        }

        private void CenterToPlayerLocation()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            toastViewController.CloseCurrentToast();

            mapCamera.TranslateTo(
                coordinates: Utils.WorldToGridPosition(playerPlayerWorldPosition.Get()),
                duration: TRANSLATION_DURATION);

            exploreV2Analytics.SendCenterMapToPlayer();
        }
    }
}
