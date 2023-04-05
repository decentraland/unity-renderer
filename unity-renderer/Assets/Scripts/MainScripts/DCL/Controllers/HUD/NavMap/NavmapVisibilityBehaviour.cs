using System;
using DCL.Helpers;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using UnityEngine;

namespace DCL
{
    public class NavmapVisibilityBehaviour : IDisposable
    {
        private static readonly MapLayer ACTIVE_MAP_LAYERS =
            MapLayer.Atlas | MapLayer.HomePoint | MapLayer.ScenesOfInterest | MapLayer.PlayerMarker
            | MapLayer.HotUsersMarkers | MapLayer.ColdUsersMarkers | MapLayer.ParcelHoverHighlight;

        private Vector3 atlasOriginalPosition;

        private bool waitingForFullscreenHUDOpen;

        private readonly BaseVariable<bool> navmapVisible;

        private readonly NavmapZoom zoom;
        private readonly NavmapRendererConfiguration rendererConfiguration;

        private readonly NavmapToastViewController navmapToastViewController;
        private readonly NavmapZoomViewController navmapZoomViewController;
        private readonly MapCameraDragBehavior mapCameraDragBehavior;

        private Service<IMapRenderer> mapRenderer;

        private readonly BaseVariable<bool> navmapIsRendered = DataStore.i.HUDs.navmapIsRendered;

        private IMapCameraController cameraController;
        private Camera hudCamera => DataStore.i.camera.hudsCamera.Get();

        public NavmapVisibilityBehaviour(BaseVariable<bool> navmapVisible, NavmapZoom zoom, NavmapToastView toastView,
            NavmapRendererConfiguration rendererConfiguration)
        {
            this.navmapVisible = navmapVisible;

            this.zoom = zoom;
            this.rendererConfiguration = rendererConfiguration;

            DataStore.i.exploreV2.isOpen.OnChange += OnExploreOpenChanged;
            navmapVisible.OnChange += OnNavmapVisibilityChanged;

            navmapToastViewController = new NavmapToastViewController(MinimapMetadata.GetMetadata(), toastView, rendererConfiguration.RenderImage);
            navmapZoomViewController = new NavmapZoomViewController(zoom);

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
        }

        private void ReleaseCameraController()
        {
            if (cameraController != null)
            {
                cameraController.Release();
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
                        ACTIVE_MAP_LAYERS,
                        Utils.WorldToGridPosition(DataStore.i.player.playerWorldPosition.Get()),
                        navmapZoomViewController.ResetZoom(),
                        rendererConfiguration.PixelPerfectMapRendererTextureProvider.GetPixelPerfectTextureResolution(),
                        zoom.zoomVerticalRange));

                SetRenderImageTransparency(false);

                navmapToastViewController.Activate();
                navmapZoomViewController.Activate(cameraController);
                rendererConfiguration.RenderImage.Activate(hudCamera, cameraController.GetRenderTexture(), cameraController);
                rendererConfiguration.PixelPerfectMapRendererTextureProvider.Activate(cameraController);

                navmapIsRendered.Set(true);
            }
            else if (cameraController != null)
            {
                navmapToastViewController.Deactivate();
                navmapZoomViewController.Deactivate();
                rendererConfiguration.PixelPerfectMapRendererTextureProvider.Deactivate();
                rendererConfiguration.RenderImage.Deactivate();

                SetRenderImageTransparency(true);

                cameraController.Release();
                cameraController = null;

                navmapIsRendered.Set(false);
            }
        }
    }
}
