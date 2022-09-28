using System;
using System.Collections.Generic;
using DCL.Helpers;
using KernelConfigurationTypes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DCL
{
    public class MapRenderer : MonoBehaviour
    {
        const int LEFT_BORDER_PARCELS = 25;
        const int RIGHT_BORDER_PARCELS = 31;
        const int TOP_BORDER_PARCELS = 31;
        const int BOTTOM_BORDER_PARCELS = 25;
        const int WORLDMAP_WIDTH_IN_PARCELS = 300;
        const string MINIMAP_USER_ICONS_POOL_NAME = "MinimapUserIconsPool";
        const int MINIMAP_USER_ICONS_MAX_PREWARM = 30;
        private const int MAX_CURSOR_PARCEL_DISTANCE = 40;
        private const int MAX_SCENE_CHARACTER_TITLE = 29;
        private const string EMPTY_PARCEL_NAME = "Empty parcel";

        public static System.Action<int, int> OnParcelClicked;
        public static System.Action OnCursorFarFromParcel;

        [SerializeField] private float parcelHightlightScale = 1.25f;
        [SerializeField] private Button ParcelHighlightButton;
        [SerializeField] private MapParcelHighlight highlight;
        [SerializeField] private Image parcelHighlightImage;
        [SerializeField] private Image parcelHighlighImagePrefab;
        [SerializeField] private Image parcelHighlighWithContentImagePrefab;
        [SerializeField] private Image selectParcelHighlighImagePrefab;

        [HideInInspector] public Vector2Int cursorMapCoords;
        [HideInInspector] public bool showCursorCoords = true;
        public MapAtlas atlas;
        public TextMeshProUGUI highlightedParcelText;
        public Transform overlayContainer;
        public Transform overlayContainerPlayers;
        public Transform globalUserMarkerContainer;
        public RectTransform playerPositionIcon;

        public float scaleFactor = 1f;

        // Used as a reference of the coordinates origin in-map and as a parcel width/height reference
        public RectTransform centeredReferenceParcel;

        public MapSceneIcon scenesOfInterestIconPrefab;
        public GameObject userIconPrefab;
        public GameObject homePointIconPrefab;
        public UserMarkerObject globalUserMarkerPrefab;
        private Dictionary<Vector2Int, Image> highlightedLands = new Dictionary<Vector2Int, Image>();
        private BaseVariable<Vector2Int> homePointCoordinates = DataStore.i.HUDs.homePoint;
        private RectTransform homePointIcon;

        private bool isInitialized = false;

        private Vector2Int lastClickedCursorMapCoords;
        private Vector3 lastPlayerPosition = new Vector3(float.NegativeInfinity, 0, float.NegativeInfinity);
        private Vector2Int lastSelectedLand;
        private int NAVMAP_CHUNK_LAYER;
        private bool otherPlayersIconsEnabled = true;
        private List<Vector2Int> ownedEmptyLands = new List<Vector2Int>();
        private List<Vector2Int> ownedLandsWithContent = new List<Vector2Int>();

        private bool parcelHighlightEnabledValue = false;
        private BaseVariable<Vector3> playerWorldPosition = DataStore.i.player.playerWorldPosition;

        private HashSet<MinimapMetadata.MinimapSceneInfo> scenesOfInterest = new HashSet<MinimapMetadata.MinimapSceneInfo>();
        private Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject> scenesOfInterestMarkers = new Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject>();
        private PointerEventData uiRaycastPointerEventData = new PointerEventData(EventSystem.current);
        private List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
        private Dictionary<string, PoolableObject> usersInfoMarkers = new Dictionary<string, PoolableObject>();
        private Pool usersInfoPool;

        List<WorldRange> validWorldRanges = new List<WorldRange>
        {
            new WorldRange(-150, -150, 150, 150) // default range
        };

        public static MapRenderer i { get; private set; }

        private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;
        public Vector3 playerGridPosition => Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());

        public MapGlobalUsersPositionMarkerController usersPositionMarkerController { private set; get; }

        public bool parcelHighlightEnabled
        {
            set
            {
                parcelHighlightEnabledValue = value;
                parcelHighlightImage.gameObject.SetActive(parcelHighlightEnabledValue);
                MapVisibilityChanged?.Invoke(value);
            }
            get { return parcelHighlightEnabledValue; }
        }

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        private void Awake()
        {
            i = this;
            Initialize();
        }

        void Update()
        {
            if ((playerWorldPosition.Get() - lastPlayerPosition).sqrMagnitude >= 0.1f * 0.1f)
            {
                lastPlayerPosition = playerWorldPosition.Get();
                UpdateRendering(Utils.WorldToGridPositionUnclamped(lastPlayerPosition));
            }

            if (!parcelHighlightEnabled)
                return;

            UpdateCursorMapCoords();

            UpdateParcelHighlight();

            UpdateParcelHold();
        }

        public void OnDestroy() { Cleanup(); }

        [HideInInspector]
        public event System.Action<float, float> OnMovedParcelCursor;
        public event Action<bool> MapVisibilityChanged;

        public void Initialize()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            InitializeHomePointIcon();
            EnsurePools();
            atlas.InitializeChunks();
            NAVMAP_CHUNK_LAYER = LayerMask.NameToLayer("NavmapChunk");

            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += MapRenderer_OnSceneInfoUpdated;
            otherPlayers.OnAdded += OnOtherPlayersAdded;
            otherPlayers.OnRemoved += OnOtherPlayerRemoved;
            homePointCoordinates.OnChange += MoveHomePointIcon;
            MoveHomePointIcon(homePointCoordinates.Get(), new Vector2Int());

            ParcelHighlightButton.onClick.AddListener(ClickMousePositionParcel);

            playerRotation.OnChange += OnCharacterRotate;

            highlight.SetScale(parcelHightlightScale);

            usersPositionMarkerController = new MapGlobalUsersPositionMarkerController(globalUserMarkerPrefab,
                globalUserMarkerContainer,
                MapUtils.CoordsToPosition);

            usersPositionMarkerController.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.BACKGROUND);

            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        private void MoveHomePointIcon(Vector2Int current, Vector2Int previous) { homePointIcon.anchoredPosition = MapUtils.CoordsToPosition(new Vector3(current.x, current.y, 0)); }

        private void InitializeHomePointIcon()
        {
            homePointIcon = GameObject.Instantiate(homePointIconPrefab).GetComponent<RectTransform>();
            homePointIcon.gameObject.transform.SetParent(overlayContainer.transform, false);
            homePointIcon.anchoredPosition = new Vector2(0, 0);
            homePointIcon.transform.localPosition = new Vector3(homePointIcon.transform.localPosition.x, homePointIcon.transform.localPosition.y, 0);
            homePointIcon.localScale = new Vector3(2, 2, 2);
            homePointIcon.transform.SetAsFirstSibling();
        }

        private void EnsurePools()
        {
            usersInfoPool = PoolManager.i.GetPool(MINIMAP_USER_ICONS_POOL_NAME);

            if (usersInfoPool == null)
            {
                usersInfoPool = PoolManager.i.AddPool(
                    MINIMAP_USER_ICONS_POOL_NAME,
                    Instantiate(userIconPrefab.gameObject, overlayContainerPlayers.transform),
                    maxPrewarmCount: MINIMAP_USER_ICONS_MAX_PREWARM,
                    isPersistent: true);

                if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                    usersInfoPool.ForcePrewarm();
            }
        }

        public void SetParcelHighlightActive(bool isAtive) => parcelHighlightImage.enabled = isAtive;

        public Vector3 GetParcelHighlightTransform() => parcelHighlightImage.transform.position;

        public void SetOtherPlayersIconActive(bool isActive)
        {
            otherPlayersIconsEnabled = isActive;

            foreach (PoolableObject poolableObject in usersInfoMarkers.Values)
            {
                poolableObject.gameObject.SetActive(isActive);
            }
        }

        public void SetPlayerIconActive(bool isActive) => playerPositionIcon.gameObject.SetActive(isActive);

        public void SetHighlighSize(Vector2Int size) { highlight.ChangeHighlighSize(size); }

        public void SetHighlightStyle(MapParcelHighlight.HighlighStyle style) { highlight.SetStyle(style); }

        public void Cleanup()
        {
            if (atlas != null)
                atlas.Cleanup();

            foreach (var kvp in scenesOfInterestMarkers)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);
            }

            CleanLandsHighlights();
            ClearLandHighlightsInfo();

            scenesOfInterestMarkers.Clear();

            playerRotation.OnChange -= OnCharacterRotate;
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= MapRenderer_OnSceneInfoUpdated;
            otherPlayers.OnAdded -= OnOtherPlayersAdded;
            otherPlayers.OnRemoved -= OnOtherPlayerRemoved;
            homePointCoordinates.OnChange -= MoveHomePointIcon;

            ParcelHighlightButton.onClick.RemoveListener(ClickMousePositionParcel);

            usersPositionMarkerController?.Dispose();

            KernelConfig.i.OnChange -= OnKernelConfigChanged;

            isInitialized = false;
        }

        public void CleanLandsHighlights()
        {
            foreach (KeyValuePair<Vector2Int, Image> kvp in highlightedLands)
            {
                Destroy(kvp.Value.gameObject);
            }

            highlightedLands.Clear (); //To Clear out the dictionary
        }

        public void ClearLandHighlightsInfo()
        {
            ownedLandsWithContent.Clear (); //To Clear out the content lands
            ownedEmptyLands.Clear (); //To Clear out the empty content 
        }

        public void SelectLand(Vector2Int coordsToSelect, Vector2Int size )
        {
            if (highlightedLands.ContainsKey(lastSelectedLand))
            {
                Destroy(highlightedLands[lastSelectedLand].gameObject);
                highlightedLands.Remove(lastSelectedLand);
            }

            HighlightLands(ownedEmptyLands, ownedLandsWithContent);

            if (highlightedLands.ContainsKey(coordsToSelect))
            {
                Destroy(highlightedLands[coordsToSelect].gameObject);
                highlightedLands.Remove(coordsToSelect);
            }

            CreateHighlightParcel(selectParcelHighlighImagePrefab, coordsToSelect, size);
            lastSelectedLand = coordsToSelect;
        }

        public void HighlightLands(List<Vector2Int> landsToHighlight, List<Vector2Int> landsToHighlightWithContent)
        {
            CleanLandsHighlights();

            foreach (Vector2Int coords in landsToHighlight)
            {
                if (highlightedLands.ContainsKey(coords))
                    continue;

                CreateHighlightParcel(parcelHighlighImagePrefab, coords, Vector2Int.one);
            }

            foreach (Vector2Int coords in landsToHighlightWithContent)
            {
                if (highlightedLands.ContainsKey(coords))
                    continue;

                if (!ownedLandsWithContent.Contains(coords))
                    ownedLandsWithContent.Add(coords);
                CreateHighlightParcel(parcelHighlighWithContentImagePrefab, coords, Vector2Int.one);
            }

            ownedEmptyLands = landsToHighlight;
            ownedLandsWithContent = landsToHighlightWithContent;
        }

        private void CreateHighlightParcel(Image prefab, Vector2Int coords, Vector2Int size)
        {
            var highlightItem = Instantiate(prefab, overlayContainer, true).GetComponent<Image>();
            highlightItem.rectTransform.localScale = new Vector3(parcelHightlightScale * size.x, parcelHightlightScale * size.y, 1f);
            highlightItem.rectTransform.SetAsLastSibling();
            highlightItem.rectTransform.anchoredPosition = MapUtils.CoordsToPosition(coords);
            highlightedLands.Add(coords, highlightItem);
        }

        void UpdateCursorMapCoords()
        {
            if (!IsCursorOverMapChunk())
                return;

            const int OFFSET = -60; //Map is a bit off centered, we need to adjust it a little.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(atlas.chunksParent, Input.mousePosition, DataStore.i.camera.hudsCamera.Get(), out var mapPoint);
            mapPoint -= Vector2.one * OFFSET;
            mapPoint -= (atlas.chunksParent.sizeDelta / 2f);
            cursorMapCoords = Vector2Int.RoundToInt(mapPoint / MapUtils.PARCEL_SIZE);
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

            string previousText = highlightedParcelText.text;
            parcelHighlightImage.rectTransform.SetAsLastSibling();
            parcelHighlightImage.rectTransform.anchoredPosition = MapUtils.CoordsToPosition(cursorMapCoords);
            highlightedParcelText.text = showCursorCoords ? $"{cursorMapCoords.x}, {cursorMapCoords.y}" : string.Empty;

            if (highlightedParcelText.text != previousText && !Input.GetMouseButton(0))
            {
                OnMovedParcelCursor?.Invoke(cursorMapCoords.x, cursorMapCoords.y);
            }

            // ----------------------------------------------------
            // TODO: Use sceneInfo to highlight whole scene parcels and populate scenes hover info on navmap once we can access all the scenes info
            // var sceneInfo = mapMetadata.GetSceneInfo(cursorMapCoords.x, cursorMapCoords.y);
        }

        void UpdateParcelHold()
        {
            if (Vector2.Distance(lastClickedCursorMapCoords, cursorMapCoords) > MAX_CURSOR_PARCEL_DISTANCE / (scaleFactor * 2.5f))
            {
                OnCursorFarFromParcel?.Invoke();
            }
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { validWorldRanges = current.validWorldRanges; }

        bool CoordinatesAreInsideTheWorld(int xCoord, int yCoord)
        {
            foreach (WorldRange worldRange in validWorldRanges)
            {
                if (worldRange.Contains(xCoord, yCoord))
                {
                    return true;
                }
            }
            return false;
        }

        private void MapRenderer_OnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!sceneInfo.isPOI)
                return;

            if (scenesOfInterest.Contains(sceneInfo))
                return;

            if (IsEmptyParcel(sceneInfo))
                return;

            scenesOfInterest.Add(sceneInfo);

            GameObject go = Object.Instantiate(scenesOfInterestIconPrefab.gameObject, overlayContainer.transform);

            Vector2 centerTile = Vector2.zero;

            foreach (var parcel in sceneInfo.parcels)
            {
                centerTile += parcel;
            }

            centerTile /= (float)sceneInfo.parcels.Count;
            float distance = float.PositiveInfinity;
            Vector2 centerParcel = Vector2.zero;
            foreach (var parcel in sceneInfo.parcels)
            {
                if (Vector2.Distance(centerTile, parcel) < distance)
                {
                    distance = Vector2.Distance(centerParcel, parcel);
                    centerParcel = parcel;
                }

            }

            (go.transform as RectTransform).anchoredPosition = MapUtils.CoordsToPosition(centerParcel);

            MapSceneIcon icon = go.GetComponent<MapSceneIcon>();

            if (icon.title != null)
                icon.title.text = sceneInfo.name.Length > MAX_SCENE_CHARACTER_TITLE ? sceneInfo.name.Substring(0, MAX_SCENE_CHARACTER_TITLE - 1) : sceneInfo.name;

            scenesOfInterestMarkers.Add(sceneInfo, go);
        }

        public void SetPointOfInterestActive(bool areActive)
        {
            foreach (GameObject pointOfInterestGameObject in scenesOfInterestMarkers.Values)
            {
                pointOfInterestGameObject.SetActive(areActive);
            }
        }

        private bool IsEmptyParcel(MinimapMetadata.MinimapSceneInfo sceneInfo) { return (sceneInfo.name != null && sceneInfo.name.Equals(EMPTY_PARCEL_NAME)); }

        private void OnOtherPlayersAdded(string userId, Player player)
        {
            var poolable = usersInfoPool.Get();
            var marker = poolable.gameObject.GetComponent<MapUserIcon>();
            marker.gameObject.name = $"UserIcon-{player.name}";
            marker.gameObject.transform.SetParent(overlayContainerPlayers.transform, true);
            marker.Populate(player);
            marker.gameObject.SetActive(otherPlayersIconsEnabled);
            marker.transform.localScale = Vector3.one;
            usersInfoMarkers.Add(userId, poolable);
        }

        private void OnOtherPlayerRemoved(string userId, Player player)
        {
            if (!usersInfoMarkers.TryGetValue(userId, out PoolableObject go))
            {
                return;
            }

            usersInfoPool.Release(go);
            usersInfoMarkers.Remove(userId);
        }

        private void ConfigureUserIcon(GameObject iconGO, Vector3 pos)
        {
            var gridPosition = Utils.WorldToGridPositionUnclamped(pos);
            iconGO.transform.localPosition = MapUtils.CoordsToPosition(Vector2Int.RoundToInt(gridPosition));
        }

        private void OnCharacterRotate(Vector3 current, Vector3 previous) { UpdateRendering(Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get())); }

        public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
        {
            if (oldCoords == newCoords)
                return;

            UpdateRendering(new Vector2((float)newCoords.x, (float)newCoords.y));
        }

        public void UpdateRendering(Vector2 newCoords)
        {
            UpdateBackgroundLayer(newCoords);
            UpdateSelectionLayer();
            UpdateOverlayLayer();
        }

        void UpdateBackgroundLayer(Vector2 newCoords) { atlas.CenterToTile(newCoords); }

        void UpdateSelectionLayer()
        {
            //TODO(Brian): Build and place here the scene highlight if applicable.
        }

        void UpdateOverlayLayer()
        {
            //NOTE(Brian): Player icon
            Vector3 f = CommonScriptableObjects.cameraForward.Get();
            Quaternion playerAngle = Quaternion.Euler(0, 0, Mathf.Atan2(-f.x, f.z) * Mathf.Rad2Deg);

            var gridPosition = playerGridPosition;
            playerPositionIcon.anchoredPosition = MapUtils.CoordsToPositionWithOffset(gridPosition);
            playerPositionIcon.rotation = playerAngle;
        }

        // Called by the parcelhighlight image button
        public void ClickMousePositionParcel()
        {
            highlightedParcelText.text = string.Empty;
            lastClickedCursorMapCoords = cursorMapCoords;
            OnParcelClicked?.Invoke(cursorMapCoords.x, cursorMapCoords.y);
        }
    }
}