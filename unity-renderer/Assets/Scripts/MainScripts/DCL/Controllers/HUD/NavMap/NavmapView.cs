using System;
using System.Collections;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        private const float MOUSE_WHEEL_THRESHOLD = 0.04f;
        private const float MAP_ZOOM_LEVELS = 4;

        [Header("References")]
        [SerializeField] internal Button closeButton;
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] internal Transform scrollRectContentTransform;
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;
        [SerializeField] internal NavmapToastView toastView;
        [SerializeField] internal InputAction_Measurable mouseWheelAction;

        [Header("Zoom")]
        [SerializeField] internal InputAction_Hold zoomIn;
        [SerializeField] internal InputAction_Hold zoomOut;
        [SerializeField] internal Button zoomInButton;
        [SerializeField] internal Button zoomOutButton;
        [SerializeField] internal Image zoomInPlus;
        [SerializeField] internal Image zoomOutMinus;
        [SerializeField] internal AnimationCurve zoomCurve;

        private Vector3 atlasOriginalPosition;
        private RectTransform containerRectTransform;
        private int currentZoomLevel;
        private Color disabledColor = new Color(0f, 0f, 0f, 0.5f);
        private bool isScaling = false;
        private MinimapMetadata mapMetadata;
        private Transform mapRendererMinimapParent;
        private RectTransform minimapViewport;

        private Color normalColor = new Color(0f, 0f, 0f, 1f);
        Vector3 previousScaleSize;
        private float scale = 1f;
        private float scaleDuration = 0.2f;

        private InputAction_Trigger.Triggered selectParcelDelegate;
        private bool waitingForFullscreenHUDOpen = false;

        public BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;

        private BaseVariable<Transform> configureMapInFullscreenMenu => DataStore.i.exploreV2.configureMapInFullscreenMenu;

        void Start()
        {
            mapMetadata = MinimapMetadata.GetMetadata();
            containerRectTransform = scrollRectContentTransform.GetComponent<RectTransform>();

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
            DataStore.i.exploreV2.isOpen.OnChange += OnExploreChange;
            navmapVisible.OnChange += OnNavmapVisibleChanged;

            configureMapInFullscreenMenu.OnChange += ConfigureMapInFullscreenMenuChanged;
            ConfigureMapInFullscreenMenuChanged(configureMapInFullscreenMenu.Get(), null);
            mouseWheelAction.OnValueChanged += OnMouseWheelChangeValue;
            zoomIn.OnStarted += OnZoomPlusMinus;
            zoomOut.OnStarted += OnZoomPlusMinus;
            zoomInButton.onClick.AddListener(() =>
            {
                OnZoomPlusMinus(DCLAction_Hold.ZoomIn);
            });
            zoomOutButton.onClick.AddListener(() =>
            {
                OnZoomPlusMinus(DCLAction_Hold.ZoomOut);
            });
            ResetCameraZoom();
            Initialize();
        }

        private void OnDestroy()
        {
            MapRenderer.OnParcelClicked -= TriggerToast;
            MapRenderer.OnCursorFarFromParcel -= CloseToast;
            CommonScriptableObjects.playerCoords.OnChange -= UpdateCurrentSceneData;
            navmapVisible.OnChange -= OnNavmapVisibleChanged;
            configureMapInFullscreenMenu.OnChange -= ConfigureMapInFullscreenMenuChanged;
            mouseWheelAction.OnValueChanged -= OnMouseWheelChangeValue;
            zoomIn.OnStarted -= OnZoomPlusMinus;
            zoomOut.OnStarted -= OnZoomPlusMinus;
            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= IsFullscreenHUDOpen_OnChange;
            DataStore.i.exploreV2.isOpen.OnChange -= OnExploreChange;
        }

        public event Action<bool> OnToggle;

        public void Initialize()
        {
            toastView.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
            DataStore.i.HUDs.isNavMapInitialized.Set(true);
        }

        public void SetExitButtonActive(bool isActive) =>
            closeButton.gameObject.SetActive(isActive);

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

        private void ResetCameraZoom()
        {
            currentZoomLevel = Mathf.FloorToInt(MAP_ZOOM_LEVELS / 2);
            scale = zoomCurve.Evaluate(currentZoomLevel);
            containerRectTransform.localScale = new Vector3(scale, scale, scale);
            HandleZoomButtonsAspect();
        }

        private void OnZoomPlusMinus(DCLAction_Hold action)
        {
            if (!navmapVisible.Get())
                return;

            if (action.Equals(DCLAction_Hold.ZoomIn))
            {
                CalculateZoomLevelAndDirection(1);
            }
            else if (action.Equals(DCLAction_Hold.ZoomOut))
            {
                CalculateZoomLevelAndDirection(-1);
            }
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void OnMouseWheelChangeValue(DCLAction_Measurable action, float value)
        {
            if (value > -MOUSE_WHEEL_THRESHOLD && value < MOUSE_WHEEL_THRESHOLD)
                return;
            CalculateZoomLevelAndDirection(value);
        }

        private void CalculateZoomLevelAndDirection(float value)
        {
            if (!navmapVisible.Get())
                return;
            if (isScaling)
                return;
            previousScaleSize = new Vector3(scale, scale, scale);
            if (value > 0 && currentZoomLevel < MAP_ZOOM_LEVELS)
            {
                currentZoomLevel++;
                StartCoroutine(ScaleOverTime(previousScaleSize));
            }
            if (value < 0 && currentZoomLevel >= 1)
            {
                currentZoomLevel--;
                StartCoroutine(ScaleOverTime(previousScaleSize));
            }
            HandleZoomButtonsAspect();
        }

        private void HandleZoomButtonsAspect()
        {
            if (currentZoomLevel < MAP_ZOOM_LEVELS)
            {
                zoomInButton.interactable = true;
                zoomInPlus.color = normalColor;
            }
            else
            {
                zoomInButton.interactable = false;
                zoomInPlus.color = disabledColor;
            }

            if (currentZoomLevel >= 1)
            {
                zoomOutButton.interactable = true;
                zoomOutMinus.color = normalColor;
            }
            else
            {
                zoomOutButton.interactable = false;
                zoomOutMinus.color = disabledColor;
            }
        }

        private IEnumerator ScaleOverTime(Vector3 startScaleSize)
        {
            isScaling = true;
            scale = zoomCurve.Evaluate(currentZoomLevel);
            MapRenderer.i.scaleFactor = scale;
            Vector3 targetScale = new Vector3(scale, scale, scale);

            float counter = 0;

            while (counter < scaleDuration)
            {
                counter += Time.deltaTime;
                containerRectTransform.localScale = Vector3.Lerp(startScaleSize, targetScale, counter / scaleDuration);
                yield return null;
            }

            isScaling = false;
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
                    CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= IsFullscreenHUDOpen_OnChange;
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

                MapRenderer.i.scaleFactor = scale;

                if (minimapViewport == null)
                    minimapViewport = MapRenderer.i.atlas.viewport;

                if (mapRendererMinimapParent == null)
                    mapRendererMinimapParent = MapRenderer.i.transform.parent;

                atlasOriginalPosition = MapRenderer.i.atlas.chunksParent.transform.localPosition;

                MapRenderer.i.atlas.viewport = scrollRect.viewport;
                MapRenderer.i.transform.SetParent(scrollRectContentTransform);
                MapRenderer.i.atlas.UpdateCulling();

                scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;

                // Center map
                MapRenderer.i.atlas.CenterToTile(Utils.WorldToGridPositionUnclamped(DataStore.i.player.playerWorldPosition.Get()));

                // Set shorter interval of time for populated scenes markers fetch
                MapRenderer.i.usersPositionMarkerController?.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.FOREGROUND);
            }
            else
            {
                if (minimapViewport == null)
                    return;
                ResetCameraZoom();
                CloseToast();

                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(DataStore.i.player.playerWorldPosition.Get()));

                // Set longer interval of time for populated scenes markers fetch
                MapRenderer.i.usersPositionMarkerController?.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.BACKGROUND);
            }

            OnToggle?.Invoke(visible);
        }

        private void UpdateCurrentSceneData(Vector2Int current, Vector2Int previous)
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

        private void ConfigureMapInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            SetAsFullScreenMenuMode(currentParentTransform);
    }
}