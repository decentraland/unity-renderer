using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DCL
{
    public class MapRenderer : MonoBehaviour
    {
        const int LEFT_BORDER_PARCELS = 25;
        const int RIGHT_BORDER_PARCELS = 31;
        const int TOP_BORDER_PARCELS = 31;
        const int BOTTOM_BORDER_PARCELS = 25;
        const int WORLDMAP_WIDTH_IN_PARCELS = 300;
        const string MINIMAP_USER_ICONS_POOL_NAME = "MinimapUserIcons";
        private int NAVMAP_CHUNK_LAYER;

        public static MapRenderer i { get; private set; }

        [SerializeField] private float parcelHightlightScale = 1.25f;
        [SerializeField] private float parcelHoldTimeInSeconds = 1f;
        [SerializeField] private Button ParcelHighlightButton;
        private float parcelSizeInMap;
        private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;
        private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;
        private Vector3[] mapWorldspaceCorners = new Vector3[4];
        private Vector3 worldCoordsOriginInMap;
        private Vector3 lastCursorMapCoords;
        private float parcelHoldCountdown;
        private List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
        private PointerEventData uiRaycastPointerEventData = new PointerEventData(EventSystem.current);

        [HideInInspector] public Vector3 cursorMapCoords;
        [HideInInspector] public bool showCursorCoords = true;
        public Vector3 playerGridPosition => Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());
        public MapAtlas atlas;
        public RawImage parcelHighlightImage;
        public TextMeshProUGUI highlightedParcelText;
        public Transform overlayContainer;

        public Image playerPositionIcon;

        // Used as a reference of the coordinates origin in-map and as a parcel width/height reference
        public RectTransform centeredReferenceParcel;

        public MapSceneIcon scenesOfInterestIconPrefab;
        public MapSceneIcon userIconPrefab;

        private HashSet<MinimapMetadata.MinimapSceneInfo> scenesOfInterest = new HashSet<MinimapMetadata.MinimapSceneInfo>();
        private Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject> scenesOfInterestMarkers = new Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject>();
        private Dictionary<string, MinimapMetadata.MinimapUserInfo> usersInfo = new Dictionary<string, MinimapMetadata.MinimapUserInfo>();
        private Dictionary<string, PoolableObject> usersInfoMarkers = new Dictionary<string, PoolableObject>();
        private Pool usersInfoPool;

        private bool parcelHighlightEnabledValue = false;
        public bool parcelHighlightEnabled
        {
            set
            {
                parcelHighlightEnabledValue = value;
                parcelHighlightImage.gameObject.SetActive(parcelHighlightEnabledValue);
            }
            get
            {
                return parcelHighlightEnabledValue;
            }
        }

        public static System.Action<int, int> OnParcelClicked;
        public static System.Action<int, int> OnParcelHold;
        public static System.Action OnParcelHoldCancel;

        private void Awake()
        {
            i = this;

            NAVMAP_CHUNK_LAYER = LayerMask.NameToLayer("NavmapChunk");

            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += MapRenderer_OnSceneInfoUpdated;
            MinimapMetadata.GetMetadata().OnUserInfoUpdated += MapRenderer_OnUserInfoUpdated;
            MinimapMetadata.GetMetadata().OnUserInfoRemoved += MapRenderer_OnUserInfoRemoved;

            ParcelHighlightButton.onClick.AddListener(() => { ClickMousePositionParcel(); });

            playerWorldPosition.OnChange += OnCharacterMove;
            playerRotation.OnChange += OnCharacterRotate;

            parcelHighlightImage.rectTransform.localScale = new Vector3(parcelHightlightScale, parcelHightlightScale, 1f);

            parcelHoldCountdown = parcelHoldTimeInSeconds;
        }

        void Update()
        {
            if (!parcelHighlightEnabled) return;

            parcelSizeInMap = centeredReferenceParcel.rect.width * centeredReferenceParcel.lossyScale.x;

            // the reference parcel has a bottom-left pivot
            centeredReferenceParcel.GetWorldCorners(mapWorldspaceCorners);
            worldCoordsOriginInMap = mapWorldspaceCorners[0];

            UpdateCursorMapCoords();

            UpdateParcelHighlight();

            UpdateParcelHold();

            lastCursorMapCoords = cursorMapCoords;
        }

        void UpdateCursorMapCoords()
        {
            if (!IsCursorOverMapChunk()) return;

            cursorMapCoords = Input.mousePosition - worldCoordsOriginInMap;
            cursorMapCoords = cursorMapCoords / parcelSizeInMap;

            cursorMapCoords.x = (int)Mathf.Floor(cursorMapCoords.x);
            cursorMapCoords.y = (int)Mathf.Floor(cursorMapCoords.y);
        }

        bool IsCursorOverMapChunk()
        {
            uiRaycastPointerEventData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(uiRaycastPointerEventData, uiRaycastResults);

            return uiRaycastResults.Count > 0 && uiRaycastResults[0].gameObject.layer == NAVMAP_CHUNK_LAYER;
        }

        void UpdateParcelHighlight()
        {
            if (!CoordinatesAreInsideTheWorld((int)cursorMapCoords.x, (int)cursorMapCoords.y))
            {
                if (parcelHighlightImage.gameObject.activeSelf)
                    parcelHighlightImage.gameObject.SetActive(false);

                return;
            }

            if (!parcelHighlightImage.gameObject.activeSelf)
                parcelHighlightImage.gameObject.SetActive(true);

            parcelHighlightImage.transform.position = worldCoordsOriginInMap + cursorMapCoords * parcelSizeInMap + new Vector3(parcelSizeInMap, parcelSizeInMap, 0f) / 2;
            highlightedParcelText.text = showCursorCoords ? $"{cursorMapCoords.x}, {cursorMapCoords.y}" : string.Empty;

            // ----------------------------------------------------
            // TODO: Use sceneInfo to highlight whole scene parcels and populate scenes hover info on navmap once we can access all the scenes info
            // var sceneInfo = mapMetadata.GetSceneInfo(cursorMapCoords.x, cursorMapCoords.y);
        }

        void UpdateParcelHold()
        {
            if (cursorMapCoords == lastCursorMapCoords)
            {
                if (parcelHoldCountdown <= 0f) return;

                parcelHoldCountdown -= Time.deltaTime;

                if (parcelHoldCountdown <= 0)
                {
                    parcelHoldCountdown = 0f;
                    highlightedParcelText.text = string.Empty;
                    OnParcelHold?.Invoke((int)cursorMapCoords.x, (int)cursorMapCoords.y);
                }
            }
            else
            {
                parcelHoldCountdown = parcelHoldTimeInSeconds;
                OnParcelHoldCancel?.Invoke();
            }
        }

        bool CoordinatesAreInsideTheWorld(int xCoord, int yCoord)
        {
            return (Mathf.Abs(xCoord) <= WORLDMAP_WIDTH_IN_PARCELS / 2) && (Mathf.Abs(yCoord) <= WORLDMAP_WIDTH_IN_PARCELS / 2);
        }

        private void MapRenderer_OnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!sceneInfo.isPOI)
                return;

            if (scenesOfInterest.Contains(sceneInfo))
                return;

            scenesOfInterest.Add(sceneInfo);

            GameObject go = Object.Instantiate(scenesOfInterestIconPrefab.gameObject, overlayContainer.transform);

            Vector2 centerTile = Vector2.zero;

            foreach (var parcel in sceneInfo.parcels)
            {
                centerTile += parcel;
            }

            centerTile /= (float)sceneInfo.parcels.Count;

            (go.transform as RectTransform).anchoredPosition = MapUtils.GetTileToLocalPosition(centerTile.x, centerTile.y);

            MapSceneIcon icon = go.GetComponent<MapSceneIcon>();
            if (icon.title != null)
                icon.title.text = sceneInfo.name;

            scenesOfInterestMarkers.Add(sceneInfo, go);
        }

        private void MapRenderer_OnUserInfoUpdated(MinimapMetadata.MinimapUserInfo userInfo)
        {
            if (usersInfo.TryGetValue(userInfo.userId, out MinimapMetadata.MinimapUserInfo existingUserInfo))
            {
                existingUserInfo = userInfo;

                if (usersInfoMarkers.TryGetValue(userInfo.userId, out PoolableObject go))
                    ConfigureUserIcon(go.gameObject, userInfo.worldPosition, userInfo.userName);
            }
            else
            {
                usersInfo.Add(userInfo.userId, userInfo);

                if (!PoolManager.i.ContainsPool(MINIMAP_USER_ICONS_POOL_NAME))
                    usersInfoPool = PoolManager.i.AddPool(MINIMAP_USER_ICONS_POOL_NAME, Instantiate(userIconPrefab.gameObject, overlayContainer.transform));

                PoolableObject newUserIcon = usersInfoPool.Get();
                newUserIcon.gameObject.name = string.Format("UserIcon-{0}", userInfo.userName);
                newUserIcon.gameObject.transform.parent = overlayContainer.transform;
                newUserIcon.gameObject.transform.localScale = Vector3.one;
                ConfigureUserIcon(newUserIcon.gameObject, userInfo.worldPosition, userInfo.userName);

                usersInfoMarkers.Add(userInfo.userId, newUserIcon);
            }
        }

        private void MapRenderer_OnUserInfoRemoved(string userId)
        {
            if (!usersInfo.Remove(userId))
                return;

            if (usersInfoMarkers.TryGetValue(userId, out PoolableObject go))
            {
                usersInfoPool.Release(go);
                usersInfoMarkers.Remove(userId);
            }
        }

        private void ConfigureUserIcon(GameObject iconGO, Vector3 pos, string name)
        {
            var gridPosition = Utils.WorldToGridPositionUnclamped(pos);
            iconGO.transform.localPosition = MapUtils.GetTileToLocalPosition(gridPosition.x, gridPosition.y);
            MapSceneIcon icon = iconGO.GetComponent<MapSceneIcon>();
            if (icon.title != null)
                icon.title.text = name;
        }

        public void OnDestroy()
        {
            playerWorldPosition.OnChange -= OnCharacterMove;
            playerRotation.OnChange -= OnCharacterRotate;
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= MapRenderer_OnSceneInfoUpdated;
            MinimapMetadata.GetMetadata().OnUserInfoUpdated -= MapRenderer_OnUserInfoUpdated;
            MinimapMetadata.GetMetadata().OnUserInfoRemoved -= MapRenderer_OnUserInfoRemoved;
        }

        private void OnCharacterMove(Vector3 current, Vector3 previous)
        {
            UpdateRendering(Utils.WorldToGridPositionUnclamped(current));
        }

        private void OnCharacterRotate(Vector3 current, Vector3 previous)
        {
            UpdateRendering(Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get()));
        }

        public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
        {
            UpdateRendering(new Vector2((float)newCoords.x, (float)newCoords.y));
        }

        public void UpdateRendering(Vector2 newCoords)
        {
            UpdateBackgroundLayer(newCoords);
            UpdateSelectionLayer();
            UpdateOverlayLayer();
        }

        void UpdateBackgroundLayer(Vector2 newCoords)
        {
            atlas.CenterToTile(newCoords);
        }

        void UpdateSelectionLayer()
        {
            //TODO(Brian): Build and place here the scene highlight if applicable.
        }

        void UpdateOverlayLayer()
        {
            //NOTE(Brian): Player icon
            Vector3 f = CommonScriptableObjects.cameraForward.Get();
            Quaternion playerAngle = Quaternion.Euler(0, 0, Mathf.Atan2(-f.x, f.z) * Mathf.Rad2Deg);

            var gridPosition = this.playerGridPosition;
            playerPositionIcon.transform.localPosition = MapUtils.GetTileToLocalPosition(gridPosition.x, gridPosition.y);
            playerPositionIcon.transform.rotation = playerAngle;
        }

        public Vector3 GetViewportCenter()
        {
            return atlas.viewport.TransformPoint(atlas.viewport.rect.center);
        }

        // Called by the parcelhighlight image button
        public void ClickMousePositionParcel()
        {
            highlightedParcelText.text = string.Empty;
            OnParcelClicked?.Invoke((int)cursorMapCoords.x, (int)cursorMapCoords.y);
        }
    }
}
