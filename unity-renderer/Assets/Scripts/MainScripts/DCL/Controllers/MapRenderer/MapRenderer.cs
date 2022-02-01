using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using KernelConfigurationTypes;

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
        private int NAVMAP_CHUNK_LAYER;

        public static MapRenderer i { get; private set; }

        [SerializeField] private float parcelHightlightScale = 1.25f;
        [SerializeField] private Button ParcelHighlightButton;
        [SerializeField] private MapParcelHighlight highlight;      
        [SerializeField] private RawImage parcelHighlightImage;
        [SerializeField] private RawImage parcelHighlighImagePrefab;
        [SerializeField] private RawImage selectParcelHighlighImagePrefab;

        private float parcelSizeInMap;
        private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;
        private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;
        private Vector3[] mapWorldspaceCorners = new Vector3[4];
        private Vector3 worldCoordsOriginInMap;
        private List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
        private PointerEventData uiRaycastPointerEventData = new PointerEventData(EventSystem.current);

        [HideInInspector] public Vector3 cursorMapCoords;
        [HideInInspector] public bool showCursorCoords = true;
        public Vector3 playerGridPosition => Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());
        public MapAtlas atlas;
        public TextMeshProUGUI highlightedParcelText;
        public Transform overlayContainer;
        public Transform globalUserMarkerContainer;

        public RectTransform playerPositionIcon;

        // Used as a reference of the coordinates origin in-map and as a parcel width/height reference
        public RectTransform centeredReferenceParcel;

        public MapSceneIcon scenesOfInterestIconPrefab;
        public GameObject userIconPrefab;
        public UserMarkerObject globalUserMarkerPrefab;

        public MapGlobalUsersPositionMarkerController usersPositionMarkerController { private set; get; }

        private HashSet<MinimapMetadata.MinimapSceneInfo> scenesOfInterest = new HashSet<MinimapMetadata.MinimapSceneInfo>();
        private Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject> scenesOfInterestMarkers = new Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject>();
        private Dictionary<string, PoolableObject> usersInfoMarkers = new Dictionary<string, PoolableObject>();

        private Vector3 lastClickedCursorMapCoords;
        private Pool usersInfoPool;

        private bool parcelHighlightEnabledValue = false;

        List<WorldRange> validWorldRanges = new List<WorldRange>
        {
            new WorldRange(-150, -150, 150, 150) // default range
        };

        public bool parcelHighlightEnabled
        {
            set
            {
                parcelHighlightEnabledValue = value;
                parcelHighlightImage.gameObject.SetActive(parcelHighlightEnabledValue);
            }
            get { return parcelHighlightEnabledValue; }
        }

        public static System.Action<int, int> OnParcelClicked;
        public static System.Action OnCursorFarFromParcel;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private Dictionary<Vector2Int, RawImage> highlightedLands = new Dictionary<Vector2Int, RawImage>();
        private Vector2Int lastSelectedLand;

        private bool isInitialized = false;

        [HideInInspector]
        public event System.Action<float, float> OnMovedParcelCursor;

        private void Awake()
        {
            i = this;
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            EnsurePools();
            atlas.InitializeChunks();

            NAVMAP_CHUNK_LAYER = LayerMask.NameToLayer("NavmapChunk");

            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += MapRenderer_OnSceneInfoUpdated;
            otherPlayers.OnAdded += OnOtherPlayersAdded;
            otherPlayers.OnRemoved += OnOtherPlayerRemoved;

            ParcelHighlightButton.onClick.AddListener(ClickMousePositionParcel);

            playerWorldPosition.OnChange += OnCharacterMove;
            playerRotation.OnChange += OnCharacterRotate;

            parcelHighlightImage.rectTransform.localScale = new Vector3(parcelHightlightScale, parcelHightlightScale, 1f);

            usersPositionMarkerController = new MapGlobalUsersPositionMarkerController(globalUserMarkerPrefab,
                globalUserMarkerContainer,
                MapUtils.GetTileToLocalPosition);

            usersPositionMarkerController.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.BACKGROUND);

            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        private void EnsurePools()
        {
            usersInfoPool = PoolManager.i.GetPool(MINIMAP_USER_ICONS_POOL_NAME);

            if (usersInfoPool == null)
            {
                usersInfoPool = PoolManager.i.AddPool(
                    MINIMAP_USER_ICONS_POOL_NAME,
                    Instantiate(userIconPrefab.gameObject, overlayContainer.transform),
                    maxPrewarmCount: MINIMAP_USER_ICONS_MAX_PREWARM,
                    isPersistent: true);

                if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                    usersInfoPool.ForcePrewarm();
            }
        }
        
        public void SetParcelHighlightActive(bool isAtive) => parcelHighlightImage.enabled = isAtive;
        
        public Vector3 GetParcelHighlightTransform() => parcelHighlightImage.transform.position;

        public void SetHighlighSize(Vector2Int size) { highlight.ChangeHighlighSize(size); }

        public void SetHighlightStyle(MapParcelHighlight.HighlighStyle style) { highlight.SetStyle(style); }

        public void OnDestroy() { Cleanup(); }

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

            scenesOfInterestMarkers.Clear();

            playerWorldPosition.OnChange -= OnCharacterMove;
            playerRotation.OnChange -= OnCharacterRotate;
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= MapRenderer_OnSceneInfoUpdated;
            otherPlayers.OnAdded -= OnOtherPlayersAdded;
            otherPlayers.OnRemoved -= OnOtherPlayerRemoved;

            ParcelHighlightButton.onClick.RemoveListener(ClickMousePositionParcel);

            usersPositionMarkerController?.Dispose();

            KernelConfig.i.OnChange -= OnKernelConfigChanged;

            isInitialized = false;
        }

        public void CleanLandsHighlights()
        {
            foreach (KeyValuePair<Vector2Int, RawImage> kvp in highlightedLands)
            {
                Destroy(kvp.Value.gameObject);
            }

            highlightedLands.Clear (); //To Clear out the dictionary
        }

        public void SelectLand(Vector2Int coordsToSelect, Vector2Int size )
        {
            if (highlightedLands.ContainsKey(lastSelectedLand))
            {
                Destroy(highlightedLands[lastSelectedLand].gameObject);
                highlightedLands.Remove(lastSelectedLand);  
                CreateHighlightParcel(parcelHighlighImagePrefab,lastSelectedLand, Vector2Int.one);
            }
            
            if (highlightedLands.ContainsKey(coordsToSelect))
            {
                Destroy(highlightedLands[coordsToSelect].gameObject);
                highlightedLands.Remove(coordsToSelect);
            }

            CreateHighlightParcel(selectParcelHighlighImagePrefab, coordsToSelect,size);

            lastSelectedLand = coordsToSelect;
        }
        
        public void HighlightLands(List<Vector2Int> landsToHighlight)
        {
            CleanLandsHighlights();

            foreach (Vector2Int coords in landsToHighlight)
            {
                if (highlightedLands.ContainsKey(coords))
                    continue;

                CreateHighlightParcel(parcelHighlighImagePrefab,coords, Vector2Int.one);
            }
        }

        private void CreateHighlightParcel(RawImage prefab,Vector2Int coords, Vector2Int size)
        {
            var highlightItem = Instantiate(prefab, overlayContainer, true).GetComponent<RawImage>();
            highlightItem.rectTransform.localScale = new Vector3(parcelHightlightScale*size.x, parcelHightlightScale*size.y, 1f);
            highlightItem.rectTransform.SetAsLastSibling();
            highlightItem.rectTransform.anchoredPosition = MapUtils.GetTileToLocalPosition(coords.x, coords.y);
            highlightedLands.Add(coords, highlightItem);
        }

        void Update()
        {
            if (!parcelHighlightEnabled)
                return;

            parcelSizeInMap = centeredReferenceParcel.rect.width * centeredReferenceParcel.lossyScale.x;

            // the reference parcel has a bottom-left pivot
            centeredReferenceParcel.GetWorldCorners(mapWorldspaceCorners);
            worldCoordsOriginInMap = mapWorldspaceCorners[0];

            UpdateCursorMapCoords();

            UpdateParcelHighlight();

            UpdateParcelHold();
        }

        void UpdateCursorMapCoords()
        {
            if (!IsCursorOverMapChunk())
                return;

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

            string previousText = highlightedParcelText.text;
            parcelHighlightImage.rectTransform.SetAsLastSibling();
            parcelHighlightImage.rectTransform.anchoredPosition = MapUtils.GetTileToLocalPosition(cursorMapCoords.x, cursorMapCoords.y);
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
            if(Vector3.Distance(lastClickedCursorMapCoords, cursorMapCoords) > MAX_CURSOR_PARCEL_DISTANCE)
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

        public void SetPointOfInterestActive(bool areActive)
        {
            foreach (GameObject pointOfInterestGameObject in scenesOfInterestMarkers.Values)
            {
                pointOfInterestGameObject.SetActive(areActive);
            }
        }

        private void OnOtherPlayersAdded(string userId, Player player)
        {
            var poolable = usersInfoPool.Get();
            var marker = poolable.gameObject.GetComponent<MapUserIcon>();
            marker.gameObject.name = $"UserIcon-{player.name}";
            marker.gameObject.transform.SetParent(overlayContainer.transform, true);
            marker.Populate(player);
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
            iconGO.transform.localPosition = MapUtils.GetTileToLocalPosition(gridPosition.x, gridPosition.y);
        }

        private void OnCharacterMove(Vector3 current, Vector3 previous)
        {
            current.y = 0;
            previous.y = 0;

            if (Vector3.Distance(current, previous) < 0.1f)
                return;

            UpdateRendering(Utils.WorldToGridPositionUnclamped(current));
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
            playerPositionIcon.anchoredPosition = MapUtils.GetTileToLocalPosition(gridPosition.x, gridPosition.y);
            playerPositionIcon.rotation = playerAngle;
        }

        public Vector3 GetViewportCenter() { return atlas.viewport.TransformPoint(atlas.viewport.rect.center); }

        // Called by the parcelhighlight image button
        public void ClickMousePositionParcel()
        {
            highlightedParcelText.text = string.Empty;
            lastClickedCursorMapCoords = new Vector3((int)cursorMapCoords.x, (int)cursorMapCoords.y, 0);
            OnParcelClicked?.Invoke((int)cursorMapCoords.x, (int)cursorMapCoords.y);
        }
    }
}