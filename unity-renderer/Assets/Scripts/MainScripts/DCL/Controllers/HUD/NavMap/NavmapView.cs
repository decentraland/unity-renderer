using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;
using DCL.Helpers;
using TMPro;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Button closeButton;
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] Transform scrollRectContentTransform;
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;
        [SerializeField] internal NavmapToastView toastView;

        InputAction_Trigger.Triggered selectParcelDelegate;
        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;
        Vector3 atlasOriginalPosition;
        MinimapMetadata mapMetadata;
        bool waitingForFullscreenHUDOpen = false;

        BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;

        public BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
        public static event System.Action<bool> OnToggle;

        void Start()
        {
            mapMetadata = MinimapMetadata.GetMetadata();

            closeButton.onClick.AddListener(() =>
            {
                navmapVisible.Set(false);
            });
            scrollRect.onValueChanged.AddListener((x) =>
            {
                if (!navmapVisible.Get())
                    return;

                MapRenderer.i.atlas.UpdateCulling();
                CloseToast();
            });

            toastView.OnGotoClicked += () => navmapVisible.Set(false);

            MapRenderer.OnParcelClicked += TriggerToast;
            MapRenderer.OnCursorFarFromParcel += CloseToast;
            CommonScriptableObjects.playerCoords.OnChange += UpdateCurrentSceneData;
            navmapVisible.OnChange += OnNavmapVisibleChanged;

            configureMapInFullscreenMenu.OnChange += ConfigureMapInFullscreenMenuChanged;
            ConfigureMapInFullscreenMenuChanged(configureMapInFullscreenMenu.Get(), null);

            Initialize();
        }

        private void OnNavmapVisibleChanged(bool current, bool previous) { SetVisible(current); }

        public void Initialize()
        {
            toastView.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
            DataStore.i.HUDs.isNavMapInitialized.Set(true);
        }

        private void OnDestroy()
        {
            MapRenderer.OnParcelClicked -= TriggerToast;
            MapRenderer.OnCursorFarFromParcel -= CloseToast;
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
            navmapVisible.OnChange -= OnNavmapVisibleChanged;
            configureMapInFullscreenMenu.OnChange -= ConfigureMapInFullscreenMenuChanged;
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
                    CommonScriptableObjects.isFullscreenHUDOpen.OnChange += IsFullscreenHUDOpen_OnChange;
                }
            }
            else
            {
                SetVisibility_Internal(false);
            }
        }

        private void IsFullscreenHUDOpen_OnChange(bool current, bool previous)
        {
            if (!current)
                return;

            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= IsFullscreenHUDOpen_OnChange;
            SetVisibility_Internal(true);
            waitingForFullscreenHUDOpen = false;
        }

        internal void SetVisibility_Internal(bool visible)
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

                minimapViewport = MapRenderer.i.atlas.viewport;
                mapRendererMinimapParent = MapRenderer.i.transform.parent;
                atlasOriginalPosition = MapRenderer.i.atlas.chunksParent.transform.localPosition;

                MapRenderer.i.atlas.viewport = scrollRect.viewport;
                MapRenderer.i.transform.SetParent(scrollRectContentTransform);
                MapRenderer.i.atlas.UpdateCulling();

                scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;

                // Reparent the player icon parent to scroll everything together
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(scrollRect.content);

                // Center map
                MapRenderer.i.atlas.CenterToTile(Utils.WorldToGridPositionUnclamped(CommonScriptableObjects.playerWorldPosition));

                // Set shorter interval of time for populated scenes markers fetch
                MapRenderer.i.usersPositionMarkerController?.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.FOREGROUND);
            }
            else
            {
                if (minimapViewport == null)
                    return;

                CloseToast();

                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                // Restore the player icon to its original parent
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(MapRenderer.i.atlas.chunksParent.transform.parent);
                (MapRenderer.i.atlas.overlayLayerGameobject.transform as RectTransform).anchoredPosition = Vector2.zero;

                MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(CommonScriptableObjects.playerWorldPosition.Get()));

                // Set longer interval of time for populated scenes markers fetch
                MapRenderer.i.usersPositionMarkerController?.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.BACKGROUND);
            }

            OnToggle?.Invoke(visible);
        }

        void UpdateCurrentSceneData(Vector2Int current, Vector2Int previous)
        {
            const string format = "{0},{1}";
            currentSceneCoordsText.text = string.Format(format, current.x, current.y);
            currentSceneNameText.text = MinimapMetadata.GetMetadata().GetSceneInfo(current.x, current.y)?.name ?? "Unnamed";
        }

        void TriggerToast(int cursorTileX, int cursorTileY)
        {
            if(toastView.isOpen)
                CloseToast();
            var sceneInfo = mapMetadata.GetSceneInfo(cursorTileX, cursorTileY);
            if (sceneInfo == null)
                WebInterface.RequestScenesInfoAroundParcel(new Vector2(cursorTileX, cursorTileY), 15);

            toastView.Populate(new Vector2Int(cursorTileX, cursorTileY), sceneInfo);
        }

        private void CloseToast() { toastView.OnCloseClick(); }

        public void SetExitButtonActive(bool isActive) { closeButton.gameObject.SetActive(isActive); }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            transform.SetParent(parentTransform);
            transform.localScale = Vector3.one;
            SetExitButtonActive(false);

            RectTransform rectTransform = transform as RectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        private void ConfigureMapInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) { SetAsFullScreenMenuMode(currentParentTransform); }
    }
}