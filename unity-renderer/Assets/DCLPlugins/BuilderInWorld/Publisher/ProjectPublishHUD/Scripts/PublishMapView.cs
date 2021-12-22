using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public class PublishMapView : MonoBehaviour
    {
        public event Action<Vector2Int> OnParcelClicked;

        [Header("References")]
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] Transform scrollRectContentTransform;
        [SerializeField] GameObject redParcelPrefab;

        InputAction_Trigger.Triggered selectParcelDelegate;
        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;
        Vector3 atlasOriginalPosition;
        MinimapMetadata mapMetadata;

        public BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
        public static event System.Action<bool> OnToggle;

        private bool isVisible = false;

        private Vector2 offset = Vector2.zero;

        void Start()
        {
            mapMetadata = MinimapMetadata.GetMetadata();

            scrollRect.onValueChanged.AddListener((x) =>
            {
                if (isVisible)
                    MapRenderer.i.atlas.UpdateCulling();
            });


            MapRenderer.OnParcelClicked += ParcelSelect;
            // MapRenderer.OnParcelHoldCancel += () => { };
            navmapVisible.OnChange += OnNavmapVisibleChanged;

            Initialize();
        }
        private void OnNavmapVisibleChanged(bool current, bool previous) { SetVisible(current); }

        public void Initialize() { StartCoroutine(WaitAndStart()); }

        private void OnDestroy()
        {
            MapRenderer.OnParcelClicked -= ParcelSelect;
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
            navmapVisible.OnChange -= OnNavmapVisibleChanged;
        }

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

        public void GoToCoords(Vector2Int coords) { MapRenderer.i.atlas.CenterToTile(coords,offset); }

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

            OnToggle?.Invoke(visible);
        }

        void UpdateCurrentSceneData(Vector2Int current, Vector2Int previous)
        {
            const string format = "{0},{1}";
            Debug.Log("current " + format);
        }

        void ParcelSelect(int cursorTileX, int cursorTileY) { OnParcelClicked?.Invoke(new Vector2Int(cursorTileX, cursorTileY)); }

        IEnumerator WaitAndStart()
        {
            yield return null;
            SetVisible(true);
        }
    }
}