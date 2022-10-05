using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DCL.MapGlobalUsersPositionMarkerController;
using static InputAction_Trigger;

namespace DCL
{

    public class NavmapView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] internal Button closeButton;
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] internal Transform scrollRectContentTransform;
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;
        [SerializeField] internal NavmapToastView toastView;

        private Vector3 atlasOriginalPosition;
        
        private MinimapMetadata mapMetadata;
        private Transform mapRendererMinimapParent;
        private RectTransform minimapViewport;

        private Triggered selectParcelDelegate;
        private bool waitingForFullscreenHUDOpen;
        
        private NavmapZoom zoom;

        private BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
        private BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;

        private void Awake()
        {
            zoom = GetComponent<NavmapZoom>();
        }

        void Start()
        {
            mapMetadata = MinimapMetadata.GetMetadata();

            closeButton.onClick.AddListener(() => { navmapVisible.Set(false); });

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
            DataStore.i.exploreV2.isOpen.OnChange += OnExploreChange;
            navmapVisible.OnChange += OnNavmapVisibleChanged;

            configureMapInFullscreenMenu.OnChange += ConfigureMapInFullscreenMenuChanged;
            ConfigureMapInFullscreenMenuChanged(configureMapInFullscreenMenu.Get(), null);
            Initialize();
        }

        private void OnDestroy()
        {
            MapRenderer.OnParcelClicked -= TriggerToast;
            MapRenderer.OnCursorFarFromParcel -= CloseToast;
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
            navmapVisible.OnChange -= OnNavmapVisibleChanged;
            configureMapInFullscreenMenu.OnChange -= ConfigureMapInFullscreenMenuChanged;
            DataStore.i.exploreV2.isOpen.OnChange -= OnExploreChange;

            if (waitingForFullscreenHUDOpen == false)
                CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= IsFullscreenHUDOpen_OnChange;
        }

        private void Initialize()
        {
            toastView.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
            DataStore.i.HUDs.isNavMapInitialized.Set(true);
        }

        private void SetExitButtonActive(bool isActive) =>
            closeButton.gameObject.SetActive(isActive);

        private void SetAsFullScreenMenuMode(Transform parentTransform)
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

        private void OnNavmapVisibleChanged(bool current, bool previous) =>
            SetVisible(current);

        private void OnExploreChange(bool current, bool previous)
        {
            if (current)
                return;

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
            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= IsFullscreenHUDOpen_OnChange;

            if (!current)
                return;

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
            else
            {
                if (minimapViewport == null)
                    return;

                zoom.ResetCameraZoom();
                CloseToast();

                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(DataStore.i.player.playerWorldPosition.Get()));

                // Set longer interval of time for populated scenes markers fetch
                MapRenderer.i.usersPositionMarkerController?.SetUpdateMode(UpdateMode.BACKGROUND);
            }
        }

        private void UpdateCurrentSceneData(Vector2Int current, Vector2Int _)
        {
            const string format = "{0},{1}";
            currentSceneCoordsText.text = string.Format(format, current.x, current.y);
            currentSceneNameText.text = MinimapMetadata.GetMetadata().GetSceneInfo(current.x, current.y)?.name ?? "Unnamed";
        }

        private void TriggerToast(int cursorTileX, int cursorTileY)
        {
            if (toastView.isOpen)
                CloseToast();
            var sceneInfo = mapMetadata.GetSceneInfo(cursorTileX, cursorTileY);
            if (sceneInfo == null)
                WebInterface.RequestScenesInfoAroundParcel(new Vector2(cursorTileX, cursorTileY), 15);

            toastView.Populate(new Vector2Int(cursorTileX, cursorTileY), sceneInfo);
        }

        private void CloseToast() =>
            toastView.OnCloseClick();

        private void ConfigureMapInFullscreenMenuChanged(Transform currentParentTransform, Transform _) =>
            SetAsFullScreenMenuMode(currentParentTransform);
    }
}