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
        [SerializeField] InputAction_Trigger toggleNavMapAction;
        [SerializeField] Button closeButton;
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] Transform scrollRectContentTransform;
        [SerializeField] internal TextMeshProUGUI currentSceneNameText;
        [SerializeField] internal TextMeshProUGUI currentSceneCoordsText;
        [SerializeField] internal NavmapToastView toastView;

        InputAction_Trigger.Triggered toggleNavMapDelegate;
        InputAction_Trigger.Triggered selectParcelDelegate;
        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;
        Vector3 atlasOriginalPosition;
        MinimapMetadata mapMetadata;
        bool cursorLockedBeforeOpening = true;

        public static bool isOpen
        {
            private set;
            get;
        } = false;

        void Start()
        {
            mapMetadata = MinimapMetadata.GetMetadata();

            closeButton.onClick.AddListener(() => { ToggleNavMap(); Utils.UnlockCursor(); });
            scrollRect.onValueChanged.AddListener((x) =>
            {
                if (!isOpen) return;

                MapRenderer.i.atlas.UpdateCulling();
                toastView.OnCloseClick();
            });

            toggleNavMapDelegate = (x) => { if (!Input.GetKeyDown(KeyCode.Escape) || isOpen) ToggleNavMap(); };
            toggleNavMapAction.OnTriggered += toggleNavMapDelegate;
            toastView.OnGotoClicked += () => ToggleNavMap(true);

            MapRenderer.OnParcelClicked += TriggerToast;
            MapRenderer.OnParcelHold += TriggerToast;
            MapRenderer.OnParcelHoldCancel += () => { toastView.OnCloseClick(); };

            MinimapHUDView.OnUpdateData += UpdateCurrentSceneData;
            MinimapHUDView.OnOpenNavmapClicked += () => ToggleNavMap();

            toastView.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            MinimapHUDView.OnUpdateData -= UpdateCurrentSceneData;
            MapRenderer.OnParcelClicked -= TriggerToast;
            MapRenderer.OnParcelHold -= TriggerToast;
        }

        internal void ToggleNavMap(bool ignoreCursorLock = false)
        {
            if (MapRenderer.i == null) return;

            scrollRect.StopMovement();

            isOpen = !isOpen;
            scrollRect.gameObject.SetActive(isOpen);
            MapRenderer.i.parcelHighlightEnabled = isOpen;

            if (isOpen)
            {
                cursorLockedBeforeOpening = Utils.isCursorLocked;
                if (!ignoreCursorLock && cursorLockedBeforeOpening)
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
            }
            else
            {
                if (!ignoreCursorLock && cursorLockedBeforeOpening)
                    Utils.LockCursor();

                toastView.OnCloseClick();

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

        void UpdateCurrentSceneData(MinimapHUDModel model)
        {
            currentSceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
            currentSceneCoordsText.text = model.playerPosition;
        }

        void TriggerToast(int cursorTileX, int cursorTileY)
        {
            var sceneInfo = mapMetadata.GetSceneInfo(cursorTileX, cursorTileY);
            if (sceneInfo == null)
                WebInterface.RequestScenesInfoAroundParcel(new Vector2(cursorTileX, cursorTileY), 1);

            toastView.Populate(new Vector2Int(cursorTileX, cursorTileY), sceneInfo);
        }
    }
}
