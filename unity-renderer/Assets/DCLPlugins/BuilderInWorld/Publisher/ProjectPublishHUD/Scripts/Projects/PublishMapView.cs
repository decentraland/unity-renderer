using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace DCL.Builder
{
    public class PublishMapView : MonoBehaviour
    {
        public event Action<Vector2Int> OnParcelClicked;

        [Header("References")]
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] RectTransform scrollRectContentTransform;

        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;
        Vector3 atlasOriginalPosition;

        private bool isVisible = false;

        private Vector2 initialOffset = Vector2.zero;

        void Start()
        {
            scrollRect.onValueChanged.AddListener((x) =>
            {
                Debug.Log("x " + x.x + "   y " + x.y);
                if (isVisible)
                    MapRenderer.i.atlas.UpdateCulling();
            });

            MapRenderer.OnParcelClicked += ParcelSelect;
        }

        private void OnDestroy() { MapRenderer.OnParcelClicked -= ParcelSelect; }

        internal void UpdateOwnedLands()
        {
            List<Vector2Int> landsToHighlight = new List<Vector2Int>();
            foreach (var land in DataStore.i.builderInWorld.landsWithAccess.Get())
            {
                foreach (Vector2Int landParcel in land.parcels)
                {
                    landsToHighlight.Add(landParcel);
                }
            }
            MapRenderer.i.HighlightLandsInRed(landsToHighlight);
        }

        public void GoToCoords(Vector2Int coords)
        {
            //Reset scroll
            Debug.Log("Before Scroll " + scrollRect.horizontalNormalizedPosition + "   Vertical " + scrollRect.verticalNormalizedPosition);

            // scrollRect.horizontalNormalizedPosition = 0.5f;
            // scrollRect.verticalNormalizedPosition = 0.5f;
            // scrollRect.normalizedPosition = new Vector2(0.5f,0.5f);

            // Debug.Log("After Scroll " + scrollRect.horizontalNormalizedPosition + "   Vertical " + scrollRect.verticalNormalizedPosition);
            scrollRectContentTransform.anchoredPosition = new Vector2(-1219, -1596);

            Debug.Log("After Anchored " + scrollRect.horizontalNormalizedPosition + "   Vertical " + scrollRect.verticalNormalizedPosition);

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
            {
                minimapViewport = MapRenderer.i.atlas.viewport;
                mapRendererMinimapParent = MapRenderer.i.transform.parent;
                atlasOriginalPosition = MapRenderer.i.atlas.chunksParent.transform.localPosition;

                MapRenderer.i.atlas.viewport = scrollRect.viewport;
                MapRenderer.i.transform.SetParent(scrollRectContentTransform);
                MapRenderer.i.atlas.UpdateCulling();

                scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;

                // Reparent the player icon parent to scroll everything together
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(scrollRect.content);

                UpdateOwnedLands();
            }
            else
            {
                MapRenderer.i.CleanRedLandsHighlights();
                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                // Restore the player icon to its original parent
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(MapRenderer.i.atlas.chunksParent.transform.parent);
                (MapRenderer.i.atlas.overlayLayerGameobject.transform as RectTransform).anchoredPosition = Vector2.zero;

                MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(CommonScriptableObjects.playerWorldPosition.Get()));
            }
        }

        void ParcelSelect(int cursorTileX, int cursorTileY) { OnParcelClicked?.Invoke(new Vector2Int(cursorTileX, cursorTileY)); }
    }
}