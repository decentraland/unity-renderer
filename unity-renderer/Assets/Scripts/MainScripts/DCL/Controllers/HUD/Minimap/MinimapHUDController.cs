using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using DCL.Tasks;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.PlacesAPIService;
using System;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public class MinimapHUDController : IHUD
{
    private static bool VERBOSE = false;

    public MinimapHUDView view;

    private FloatVariable minimapZoom => CommonScriptableObjects.minimapZoom;
    private IntVariable currentSceneNumber => CommonScriptableObjects.sceneNumber;
    private Vector2IntVariable playerCoords => CommonScriptableObjects.playerCoords;
    private Vector2Int currentCoords;
    private Vector2Int homeCoords = new Vector2Int(0, 0);

    private readonly MinimapMetadataController metadataController;
    private readonly IHomeLocationController locationController;
    private readonly DCL.Environment.Model environment;
    private readonly IPlacesAPIService placesAPIService;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly IClipboard clipboard;
    private readonly BaseVariable<bool> minimapVisible = DataStore.i.HUDs.minimapVisible;
    private readonly CancellationTokenSource disposingCts = new ();

    private static readonly MapLayer RENDER_LAYERS
        = MapLayer.Atlas | MapLayer.HomePoint | MapLayer.PlayerMarker | MapLayer.HotUsersMarkers | MapLayer.ScenesOfInterest;

    private Service<IMapRenderer> mapRenderer;
    private IMapCameraController mapCameraController;
    private MapRendererTrackPlayerPosition mapRendererTrackPlayerPosition;
    private CancellationTokenSource retrievingFavoritesCts;

    private MinimapHUDModel model { get; set; } = new ();

    public MinimapHUDController(
        MinimapMetadataController minimapMetadataController,
        IHomeLocationController locationController,
        DCL.Environment.Model environment,
        IPlacesAPIService placesAPIService,
        IPlacesAnalytics placesAnalytics,
        IClipboard clipboard)
    {
        minimapZoom.Set(1f);
        metadataController = minimapMetadataController;
        this.locationController = locationController;
        this.environment = environment;
        this.placesAPIService = placesAPIService;
        this.placesAnalytics = placesAnalytics;
        this.clipboard = clipboard;

        if (metadataController != null)
            metadataController.OnHomeChanged += SetNewHome;

        minimapVisible.OnChange += SetVisibility;
    }

    protected virtual MinimapHUDView CreateView() =>
        MinimapHUDView.Create(this);

    public void Initialize()
    {
        view = CreateView();
        view.OnFavoriteToggleClicked += OnFavoriteToggleClicked;
        view.OnCopyLocationRequested += OnCopyLocationToClipboard;
        InitializeMapRenderer();

        OnPlayerCoordsChange(CommonScriptableObjects.playerCoords.Get(), Vector2Int.zero);
        SetVisibility(minimapVisible.Get());

        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        CommonScriptableObjects.isFullscreenHUDOpen.OnChange += OnFullscreenUIVisibilityChange;
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated += OnSceneInfoUpdated;
    }

    public void Dispose()
    {
        disposingCts?.SafeCancelAndDispose();

        if (view != null)
        {
            DisposeMapRenderer();
            Object.Destroy(view.gameObject);
        }

        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChange;
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnSceneInfoUpdated;

        if (metadataController != null)
            metadataController.OnHomeChanged -= SetNewHome;

        minimapVisible.OnChange -= SetVisibility;
    }

    private void InitializeMapRenderer()
    {
        view.pixelPerfectMapRendererTextureProvider.SetHudCamera(DataStore.i.camera.hudsCamera.Get());

        mapCameraController = mapRenderer.Ref.RentCamera(
            new MapCameraInput(RENDER_LAYERS,
                Vector2Int.RoundToInt(MapRendererTrackPlayerPosition.GetPlayerCentricCoords(playerCoords.Get())),
                1,
                view.pixelPerfectMapRendererTextureProvider.GetPixelPerfectTextureResolution(),
                new Vector2Int(view.mapRendererVisibleParcels, view.mapRendererVisibleParcels)));

        mapRendererTrackPlayerPosition = new MapRendererTrackPlayerPosition(mapCameraController, DataStore.i.player.playerWorldPosition);
        view.mapRendererTargetImage.texture = mapCameraController.GetRenderTexture();
        view.pixelPerfectMapRendererTextureProvider.Activate(mapCameraController);

        DataStore.i.HUDs.navmapIsRendered.OnChange += OnNavMapIsRenderedChange;
    }

    private void OnNavMapIsRenderedChange(bool current, bool previous)
    {
        if (current)
            mapCameraController.SuspendRendering();
        else
            mapCameraController.ResumeRendering();
    }

    private void DisposeMapRenderer()
    {
        if (mapCameraController != null)
        {
            DataStore.i.HUDs.navmapIsRendered.OnChange -= OnNavMapIsRenderedChange;
            view.pixelPerfectMapRendererTextureProvider.Deactivate();
            mapRendererTrackPlayerPosition.Dispose();
            mapCameraController.Release();
            mapCameraController = null;
        }
    }

    private void OnPlayerCoordsChange(Vector2Int current, Vector2Int previous)
    {
        currentCoords = current;
        UpdatePlayerPosition(currentCoords);
        UpdateSetHomePanel();
        MinimapMetadata.MinimapSceneInfo sceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(currentCoords.x, currentCoords.y);

        if (sceneInfo != null)
        {
            UpdateSceneName(sceneInfo.name);

            if (sceneInfo.parcels.Count > 0)
                RetrieveFavoriteState(sceneInfo.parcels[0]);
        }
    }

    private void OnFullscreenUIVisibilityChange(bool current, bool previous)
    {
        OnPlayerCoordsChange(currentCoords, Vector2Int.zero);
    }

    private void SetNewHome(Vector2Int newHomeCoordinates)
    {
        homeCoords = newHomeCoordinates;
        UpdateSetHomePanel();
    }

    private void UpdateData(MinimapHUDModel model)
    {
        this.model = model;
        view.UpdateData(this.model);
    }

    public void UpdateSceneName(string sceneName)
    {
        model.sceneName = sceneName;
        view.UpdateData(model);
    }

    public void UpdatePlayerPosition(Vector2 position)
    {
        const string format = "{0},{1}";
        UpdatePlayerPosition(string.Format(format, position.x, position.y));
    }

    private void UpdateSetHomePanel()
    {
        view.UpdateSetHomePanel(currentCoords == homeCoords);
    }

    public void UpdatePlayerPosition(string position)
    {
        model.playerPosition = position;
        view.UpdateData(model);
    }

    public void AddZoomDelta(float delta)
    {
        minimapZoom.Set(Mathf.Clamp01(minimapZoom.Get() + delta));
    }

    public void ToggleOptions()
    {
        view.ToggleOptions();
    }

    public void ToggleSceneUI(bool isUIOn)
    {
        DataStore.i.HUDs.isCurrentSceneUiEnabled.Set(isUIOn);
    }

    public void AddBookmark()
    {
        //TODO:
        if (VERBOSE) { Debug.Log("Add bookmark pressed"); }
    }

    public void ReportScene()
    {
        var coords = playerCoords.Get();
        WebInterface.SendReportScene(environment.world.state.GetSceneNumberByCoords(coords));
    }

    public void SetHomeScene(bool isOn)
    {
        var coords = playerCoords.Get();

        if (playerCoords == homeCoords)
        {
            if (!isOn)
                locationController.SetHomeScene(new Vector2(0, 0));
        }
        else
        {
            if (isOn)
                locationController.SetHomeScene(new Vector2(coords.x, coords.y));
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible && minimapVisible.Get());
    }

    private void OnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
    {
        if (sceneInfo.parcels.Contains(CommonScriptableObjects.playerCoords.Get()))
        {
            UpdateSceneName(sceneInfo.name);

            if (sceneInfo.parcels.Count > 0)
                RetrieveFavoriteState(sceneInfo.parcels[0]);
        }
    }

    private void RetrieveFavoriteState(Vector2Int currentParcel)
    {
        retrievingFavoritesCts?.SafeCancelAndDispose();
        retrievingFavoritesCts = CancellationTokenSource.CreateLinkedTokenSource(disposingCts.Token);

        RetrieveFavoriteStateAsync(currentParcel, retrievingFavoritesCts.Token).Forget();
    }

    private async UniTaskVoid RetrieveFavoriteStateAsync(Vector2Int currentParcel, CancellationToken ct)
    {
        try
        {
            var place = await placesAPIService.GetPlace(currentParcel, ct);
            bool isFavorite = await placesAPIService.IsFavoritePlace(place, ct);
            view.SetIsAPlace(true);
            view.SetCurrentFavoriteStatus(place.id, isFavorite);
        }
        catch (NotAPlaceException) { view.SetIsAPlace(false); }
        catch (OperationCanceledException) { view.SetIsAPlace(false); }
    }

    private void OnFavoriteToggleClicked(string uuid, bool isFavorite)
    {
        if (isFavorite)
            placesAnalytics.AddFavorite(uuid, IPlacesAnalytics.ActionSource.FromMinimap);
        else
            placesAnalytics.RemoveFavorite(uuid, IPlacesAnalytics.ActionSource.FromMinimap);

        placesAPIService.SetPlaceFavorite(uuid, isFavorite, default).Forget();
    }

    private void SetVisibility(bool current, bool _) =>
        SetVisibility(current);

    private void OnCopyLocationToClipboard()
    {
        clipboard.WriteText($"{model.sceneName}: {model.playerPosition}");
        view.ShowLocationCopiedToast();
    }
}
