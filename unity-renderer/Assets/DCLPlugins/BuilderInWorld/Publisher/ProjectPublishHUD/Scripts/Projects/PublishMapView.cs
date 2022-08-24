using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public class PublishMapView : MonoBehaviour
    {
        private const float MAX_ZOOM = 4f;
        private const float MIN_ZOOM = 1.0f;
        private const float SIZE_PER_ZOOM = 0.5f;
        
        public event Action<Vector2Int> OnParcelClicked;
        public event Action<Vector2Int> OnParcelHover;

        [Header("References")]
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] RectTransform scrollRectContentTransform;
        [SerializeField] internal Button lessZoomBtn;
        [SerializeField] internal Button moreZoomBtn;

        private RectTransform minimapViewport;
        private RectTransform viewRectTransform;
        private Transform mapRendererMinimapParent;
        private Vector3 atlasOriginalPosition;
        private Vector2 initialContentPosition;
        private Vector2Int sceneSize;

        private bool isVisible = false;
        private bool isDragging = false;
        private float lastScale = 0;
        private float currentZoomScale = 2f;
        
        private void Start()
        {
            lessZoomBtn.onClick.AddListener(LessZoom);
            moreZoomBtn.onClick.AddListener(MoreZoom);
            
            viewRectTransform = GetComponent<RectTransform>();
            scrollRect.onValueChanged.AddListener((x) =>
            {
                if (isVisible)
                    MapRenderer.i.atlas.UpdateCulling();
            });

            MapRenderer.OnParcelClicked += ParcelSelect;
        }

        private void OnDestroy()
        {
            moreZoomBtn.onClick.RemoveAllListeners();
            lessZoomBtn.onClick.RemoveAllListeners();
            MapRenderer.OnParcelClicked -= ParcelSelect;
        }

        // Note: this event is handled by an event trigger in the same gameobject as the scrollrect
        public void BeginDrag()
        {
            isDragging = true;
            MapRenderer.i.SetParcelHighlightActive(false);
        }

        // Note: this event is handled by an event trigger in the same gameobject as the scrollrect
        public void EndDrag()
        {
            isDragging = false;
            if(RectTransformUtility.RectangleContainsScreenPoint(viewRectTransform, Input.mousePosition))
                MapRenderer.i.SetParcelHighlightActive(true);
        }

        internal void MoreZoom()
        {
            currentZoomScale = Mathf.Clamp(currentZoomScale + SIZE_PER_ZOOM, MIN_ZOOM, MAX_ZOOM);
            ApplyCurrentZoom();
        }

        internal void LessZoom()
        {
            currentZoomScale = Mathf.Clamp(currentZoomScale - SIZE_PER_ZOOM, MIN_ZOOM, MAX_ZOOM);
            ApplyCurrentZoom();
        }

        internal void ApplyCurrentZoom()
        {
            MapRenderer.i.transform.localScale = Vector3.one * currentZoomScale;
        }

        internal void UpdateOwnedLands()
        {
            List<Vector2Int> landsToHighlight = new List<Vector2Int>();
            List<Vector2Int> landsToHighlightWithContent = new List<Vector2Int>();
            foreach (var land in DataStore.i.builderInWorld.landsWithAccess.Get())
            {
                foreach (Vector2Int landParcel in land.parcels)
                {
                    bool found = false;
                    foreach (Scene scene in land.scenes)
                    {
                        if(scene.isEmpty)
                            continue;
                        
                        foreach (Vector2Int sceneParcel in scene.parcels)
                        {
                            if (sceneParcel == landParcel)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if(found)
                        landsToHighlightWithContent.Add(landParcel);
                    else
                        landsToHighlight.Add(landParcel);
                }
            }
            MapRenderer.i.HighlightLands(landsToHighlight, landsToHighlightWithContent);
        }

        public void SetProjectSize(Vector2Int[] parcels)
        {
            sceneSize = BIWUtils.GetSceneSize(parcels);
            MapRenderer.i.SetHighlighSize(sceneSize);
        }

        public void GoToCoords(Vector2Int coords)
        {
            //Reset scroll
            scrollRect.content.anchoredPosition = initialContentPosition;
            MapRenderer.i.atlas.CenterToTile(coords);
        }

        internal void SetVisible(bool visible)
        {
            if (MapRenderer.i == null || isVisible == visible)
                return;

            isVisible = visible;

            scrollRect.StopMovement();
            scrollRect.gameObject.SetActive(visible);
            MapRenderer.i.parcelHighlightEnabled = visible;

            if (visible)
                SetMapRendererInContainer();
            else
                RemoveMapRendererFromContainer();
        }

        public void SetAvailabilityToPublish(bool isAvailable)
        {
            var style = isAvailable ? MapParcelHighlight.HighlighStyle.BUILDER_ENABLE : MapParcelHighlight.HighlighStyle.BUILDER_DISABLE;
            MapRenderer.i.SetHighlightStyle(style);
        }

        private void ParcelHovered(float x, float y)
        {
            if(!isDragging)
                MapRenderer.i.SetParcelHighlightActive(true);
            OnParcelHover?.Invoke( new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y)));
        }

        private void SetMapRendererInContainer()
        {
            minimapViewport = MapRenderer.i.atlas.viewport;
            mapRendererMinimapParent = MapRenderer.i.transform.parent;
            atlasOriginalPosition = MapRenderer.i.atlas.chunksParent.transform.localPosition;

            lastScale = MapRenderer.i.transform.localScale.x;
            
            MapRenderer.i.SetHighlightStyle(MapParcelHighlight.HighlighStyle.BUILDER_DISABLE);
            MapRenderer.i.atlas.viewport = scrollRect.viewport;
            MapRenderer.i.transform.SetParent(scrollRectContentTransform);
            MapRenderer.i.atlas.UpdateCulling();
            MapRenderer.i.OnMovedParcelCursor += ParcelHovered;
            MapRenderer.i.SetPointOfInterestActive(false);
            MapRenderer.i.SetPlayerIconActive(false);
            MapRenderer.i.SetOtherPlayersIconActive(false);
            
            currentZoomScale = 2f;
            ApplyCurrentZoom();
            
            scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;
            initialContentPosition = scrollRect.content.anchoredPosition;
            
            // Reparent the player icon parent to scroll everything together
            MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(scrollRect.content);

            UpdateOwnedLands();
        }

        private void RemoveMapRendererFromContainer()
        {
            MapRenderer.i.CleanLandsHighlights();
            MapRenderer.i.ClearLandHighlightsInfo();
            MapRenderer.i.SetHighlightStyle(MapParcelHighlight.HighlighStyle.DEFAULT);
            MapRenderer.i.atlas.viewport = minimapViewport;
            MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
            MapRenderer.i.OnMovedParcelCursor -= ParcelHovered;
            MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
            MapRenderer.i.atlas.UpdateCulling();
            MapRenderer.i.SetPointOfInterestActive(true);
            MapRenderer.i.SetPlayerIconActive(true);
            MapRenderer.i.SetOtherPlayersIconActive(true);
            MapRenderer.i.transform.localScale = Vector3.one * lastScale;

            // Restore the player icon to its original parent
            MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(MapRenderer.i.atlas.chunksParent.transform.parent);
            (MapRenderer.i.atlas.overlayLayerGameobject.transform as RectTransform).anchoredPosition = Vector2.zero;

            MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(DataStore.i.player.playerWorldPosition.Get()));

        }

        public void SelectLandInMap(Vector2Int coord)
        {
            MapRenderer.i.SelectLand(coord, sceneSize);
        }

        private void ParcelSelect(int cursorTileX, int cursorTileY)
        {
            OnParcelClicked?.Invoke(new Vector2Int(cursorTileX, cursorTileY));
        }
    }
}