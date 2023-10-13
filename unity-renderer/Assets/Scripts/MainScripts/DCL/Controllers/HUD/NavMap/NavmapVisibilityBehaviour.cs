using DCL.Browser;
using System;
using DCL.Helpers;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.CommonBehavior;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.PlayerMarker;
using DCLServices.PlacesAPIService;
using ExploreV2Analytics;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class NavmapVisibilityBehaviour : IDisposable, IMapActivityOwner
    {
        private const MapLayer ACTIVE_MAP_LAYERS =
            MapLayer.SatelliteAtlas | MapLayer.ParcelsAtlas | MapLayer.HomePoint | MapLayer.ScenesOfInterest | MapLayer.PlayerMarker | MapLayer.HotUsersMarkers | MapLayer.ColdUsersMarkers | MapLayer.ParcelHoverHighlight;

        private Vector3 atlasOriginalPosition;

        private bool waitingForFullscreenHUDOpen;

        private readonly BaseVariable<bool> navmapVisible;

        private readonly NavmapZoomView zoomView;
        private readonly NavmapRendererConfiguration rendererConfiguration;

        private readonly NavmapToastViewController navmapToastViewController;
        private readonly NavmapZoomViewController navmapZoomViewController;
        private readonly NavMapLocationControlsController locationControlsController;

        private readonly NavmapSearchController navmapSearchController;
        private readonly IPlaceCardComponentView placeCardModal;
        private readonly NavMapChunksLayersController chunksLayerController;
        private readonly MapCameraDragBehavior mapCameraDragBehavior;
        private readonly NavMapChunksLayersView chunksLayersView;
        private readonly IExploreV2Analytics exploreV2Analytics;
        private readonly IBrowserBridge browserBridge;

        private Service<IMapRenderer> mapRenderer;

        private readonly BaseVariable<bool> navmapIsRendered = DataStore.i.HUDs.navmapIsRendered;

        private IMapCameraController cameraController;
        private readonly BaseVariable<FeatureFlag> featureFlagsFlags;
        public IReadOnlyDictionary<MapLayer, IMapLayerParameter> LayersParameters  { get; } = new Dictionary<MapLayer, IMapLayerParameter>
            { { MapLayer.PlayerMarker, new PlayerMarkerParameter {BackgroundIsActive = true} } };

        private Camera hudCamera => DataStore.i.camera.hudsCamera.Get();

        public NavmapVisibilityBehaviour(
            BaseVariable<FeatureFlag> featureFlagsFlags,
            BaseVariable<bool> navmapVisible,
            NavmapZoomView zoomView,
            NavmapToastView toastView,
            NavmapSearchComponentView searchView,
            NavMapLocationControlsView locationControlsView,
            NavMapChunksLayersView chunksLayersView,
            NavmapRendererConfiguration rendererConfiguration,
            IPlacesAPIService placesAPIService,
            IPlacesAnalytics placesAnalytics,
            IPlaceCardComponentView placeCardModal,
            IExploreV2Analytics exploreV2Analytics,
            IBrowserBridge browserBridge)
        {
            this.featureFlagsFlags = featureFlagsFlags;

            this.navmapVisible = navmapVisible;

            this.zoomView = zoomView;
            this.rendererConfiguration = rendererConfiguration;
            this.placeCardModal = placeCardModal;
            this.exploreV2Analytics = exploreV2Analytics;
            this.browserBridge = browserBridge;

            DataStore.i.exploreV2.isOpen.OnChange += OnExploreOpenChanged;
            navmapVisible.OnChange += OnNavmapVisibilityChanged;

            navmapToastViewController = new NavmapToastViewController(MinimapMetadata.GetMetadata(), toastView, rendererConfiguration.RenderImage, placesAPIService, placesAnalytics, this.placeCardModal, exploreV2Analytics, browserBridge);
            navmapZoomViewController = new NavmapZoomViewController(zoomView, featureFlagsFlags);
            locationControlsController = new NavMapLocationControlsController(locationControlsView, exploreV2Analytics, navmapZoomViewController, navmapToastViewController, DataStore.i.HUDs.homePoint, DataStore.i.player.playerWorldPosition);

            navmapSearchController = new NavmapSearchController(searchView, Environment.i.platform.serviceLocator.Get<IPlacesAPIService>(), new DefaultPlayerPrefs(), navmapZoomViewController, navmapToastViewController, exploreV2Analytics);
            chunksLayerController = new NavMapChunksLayersController(chunksLayersView, exploreV2Analytics);

            { // Needed for feature flag. Remove when feature flag is removed
                this.chunksLayersView = chunksLayersView;
            }

            this.rendererConfiguration.RenderImage.EmbedMapCameraDragBehavior(rendererConfiguration.MapCameraDragBehaviorData);

            SetRenderImageTransparency(true);

            rendererConfiguration.PixelPerfectMapRendererTextureProvider.SetHudCamera(hudCamera);
        }

        public void Dispose()
        {
            ReleaseCameraController();

            DataStore.i.exploreV2.isOpen.OnChange -= OnExploreOpenChanged;
            navmapVisible.OnChange -= OnNavmapVisibilityChanged;

            if (waitingForFullscreenHUDOpen == false)
                CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= OnFullScreenOpened;

            navmapToastViewController.Dispose();
            navmapZoomViewController.Dispose();
            chunksLayerController.Dispose();
        }

        private void ReleaseCameraController()
        {
            if (cameraController != null)
            {
                cameraController.Release(this);
                cameraController = null;
            }
        }

        private void SetRenderImageTransparency(bool isTransparent)
        {
            // Make Render Image transparent before activation
            var renderImageColor = this.rendererConfiguration.RenderImage.color;
            renderImageColor.a = isTransparent ? 0 : 1;
            this.rendererConfiguration.RenderImage.color = renderImageColor;
        }

        private void OnNavmapVisibilityChanged(bool isVisible, bool _) =>
            SetVisible(isVisible);

        private void OnExploreOpenChanged(bool isOpen, bool _)
        {
            if (!isOpen)
                SetVisible(false);
        }

        internal void SetVisible(bool visible)
        {
            if (waitingForFullscreenHUDOpen)
                return;

            if (visible)
            {
                if (CommonScriptableObjects.isFullscreenHUDOpen.Get())
                    SetVisibility_Internal(true);
                else
                {
                    waitingForFullscreenHUDOpen = true;
                    CommonScriptableObjects.isFullscreenHUDOpen.OnChange += OnFullScreenOpened;
                }
            }
            else
                SetVisibility_Internal(false);
        }

        private void OnFullScreenOpened(bool isFullScreen, bool _)
        {
            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= OnFullScreenOpened;

            if (!isFullScreen)
                return;

            SetVisibility_Internal(true);
            waitingForFullscreenHUDOpen = false;
        }

        private void SetVisibility_Internal(bool visible)
        {
            if (visible)
            {
                if (!DataStore.i.exploreV2.isInitialized.Get())
                    Utils.UnlockCursor();

                cameraController = mapRenderer.Ref.RentCamera(
                    new MapCameraInput(
                        this,
                        ACTIVE_MAP_LAYERS,
                        Utils.WorldToGridPosition(DataStore.i.player.playerWorldPosition.Get()),
                        navmapZoomViewController.ResetZoomToMidValue(),
                        rendererConfiguration.PixelPerfectMapRendererTextureProvider.GetPixelPerfectTextureResolution(),
                        zoomView.zoomVerticalRange
                        ));

                SetRenderImageTransparency(false);

                navmapToastViewController.Activate();
                navmapZoomViewController.Activate(cameraController);

                if (!featureFlagsFlags.Get().IsFeatureEnabled("navmap-satellite-view"))
                    chunksLayersView.Hide();

                if (featureFlagsFlags.Get().IsFeatureEnabled("map_focus_home_or_user"))
                    locationControlsController.Activate(cameraController);
                else
                    locationControlsController.Hide();

                navmapSearchController.Activate(cameraController);

                rendererConfiguration.RenderImage.Activate(hudCamera, cameraController.GetRenderTexture(), cameraController);
                rendererConfiguration.PixelPerfectMapRendererTextureProvider.Activate(cameraController);

                navmapIsRendered.Set(true);
            }
            else if (cameraController != null)
            {
                navmapToastViewController.Deactivate();
                navmapZoomViewController.Deactivate();
                locationControlsController.Deactivate();
                navmapSearchController.Deactivate();

                rendererConfiguration.PixelPerfectMapRendererTextureProvider.Deactivate();
                rendererConfiguration.RenderImage.Deactivate();

                SetRenderImageTransparency(true);

                cameraController.Release(this);
                cameraController = null;

                navmapIsRendered.Set(false);
            }
        }
    }
}
