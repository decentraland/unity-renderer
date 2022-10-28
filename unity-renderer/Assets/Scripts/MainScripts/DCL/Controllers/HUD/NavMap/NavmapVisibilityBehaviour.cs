using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;
using static DCL.MapGlobalUsersPositionMarkerController;

namespace DCL
{
    public class NavmapVisibilityBehaviour : IDisposable
    {
        private Vector3 atlasOriginalPosition;
        
        private Transform mapRendererMinimapParent;
        private RectTransform minimapViewport;

        private bool waitingForFullscreenHUDOpen;

        private readonly BaseVariable<bool> navmapVisible;
        
        private readonly ScrollRect scrollRect;
        private readonly Transform scrollRectContentTransform;
        
        private readonly NavmapZoom zoom;
        private readonly NavmapToastView toastView;

        public NavmapVisibilityBehaviour(BaseVariable<bool> navmapVisible, ScrollRect scrollRect, Transform scrollRectContentTransform, NavmapZoom zoom, NavmapToastView toastView)
        {
            this.navmapVisible = navmapVisible;
            
            this.scrollRect = scrollRect;
            this.scrollRectContentTransform = scrollRectContentTransform;
            
            this.zoom = zoom;
            this.toastView = toastView;
            
            DataStore.i.exploreV2.isOpen.OnChange += OnExploreOpenChanged;
            navmapVisible.OnChange += OnNavmapVisibilityChanged;
        }
        
        public void Dispose()
        {
            DataStore.i.exploreV2.isOpen.OnChange -= OnExploreOpenChanged;
            navmapVisible.OnChange -= OnNavmapVisibilityChanged;

            if (waitingForFullscreenHUDOpen == false)
                CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= OnFullScreenOpened;
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
                {
                    SetVisibility_Internal(true);
                }
                else
                {
                    waitingForFullscreenHUDOpen = true;
                    CommonScriptableObjects.isFullscreenHUDOpen.OnChange += OnFullScreenOpened;
                }
            }
            else
            {
                SetVisibility_Internal(false);
            }
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
            if (MapRenderer.i == null)
                return;

            scrollRect.StopMovement();

            scrollRect.gameObject.SetActive(visible);
            MapRenderer.i.parcelHighlightEnabled = visible;

            if (visible)
            {
                if (!DataStore.i.exploreV2.isInitialized.Get())
                    Utils.UnlockCursor();

                MapRenderer.i.scaleFactor = zoom.Scale;

                if (minimapViewport == null || mapRendererMinimapParent == null)
                {
                    minimapViewport = MapRenderer.i.atlas.viewport;
                    mapRendererMinimapParent = MapRenderer.i.transform.parent;
                }

                atlasOriginalPosition = MapRenderer.i.atlas.chunksParent.transform.localPosition;

                MapRenderer.i.atlas.viewport = scrollRect.viewport;
                MapRenderer.i.transform.SetParent(scrollRectContentTransform);
                MapRenderer.i.atlas.UpdateCulling();

                scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;

                // Center map
                MapRenderer.i.atlas.CenterToTile(Utils.WorldToGridPositionUnclamped(DataStore.i.player.playerWorldPosition.Get()));

                // Set shorter interval of time for populated scenes markers fetch
                MapRenderer.i.usersPositionMarkerController?.SetUpdateMode(UpdateMode.FOREGROUND);
            }
            else if (minimapViewport != null)
            {
                zoom.ResetToDefault();
                toastView.Close();

                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(DataStore.i.player.playerWorldPosition.Get()));

                // Set longer interval of time for populated scenes markers fetch
                MapRenderer.i.usersPositionMarkerController?.SetUpdateMode(UpdateMode.BACKGROUND);
            }
        }
    }
}