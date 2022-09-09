using DCL;
using DCL.Interface;
using UnityEngine;
using System;

public class MinimapHUDController : IHUD
{
    private static bool VERBOSE = false;

    public MinimapHUDView view;
    private FloatVariable minimapZoom => CommonScriptableObjects.minimapZoom;
    private StringVariable currentSceneId => CommonScriptableObjects.sceneID;
    private Vector2IntVariable playerCoords => CommonScriptableObjects.playerCoords;
    private Vector2Int currentCoords;
    private Vector2Int homeCoords = new Vector2Int(0,0);
    private MinimapMetadataController metadataController;
    private IHomeLocationController locationController;

    public MinimapHUDModel model { get; private set; } = new MinimapHUDModel();

    public MinimapHUDController(MinimapMetadataController minimapMetadataController, IHomeLocationController locationController) : this(new MinimapHUDModel(), minimapMetadataController, locationController) { }

    public MinimapHUDController(MinimapHUDModel model, MinimapMetadataController minimapMetadataController, IHomeLocationController locationController)
    {
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange += ChangeVisibilityForBuilderInWorld;
        minimapZoom.Set(1f);
        UpdateData(model);
        metadataController = minimapMetadataController;
        this.locationController = locationController;
        if(metadataController != null)
            metadataController.OnHomeChanged += SetNewHome;
    }

    protected internal virtual MinimapHUDView CreateView() { return MinimapHUDView.Create(this); }

    public void Initialize() 
    {
        view = CreateView();
        UpdateData(model);
    }

    public void Dispose()
    {
        if (view != null)
            UnityEngine.Object.Destroy(view.gameObject);

        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChange;
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange -= ChangeVisibilityForBuilderInWorld;
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnOnSceneInfoUpdated;
        
        if (metadataController != null)
            metadataController.OnHomeChanged -= SetNewHome;
    }

    private void OnPlayerCoordsChange(Vector2Int current, Vector2Int previous)
    {
        currentCoords = current;
        UpdatePlayerPosition(currentCoords);
        UpdateSetHomePanel();
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnOnSceneInfoUpdated;
        MinimapMetadata.MinimapSceneInfo sceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(currentCoords.x, currentCoords.y);
        UpdateSceneName(sceneInfo?.name);

        // NOTE: in some cases playerCoords OnChange is triggered before kernel's message with the scene info arrives.
        // so in that scenario we subscribe to MinimapMetadata event to wait for the scene info.
        if (sceneInfo == null)
        {
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += OnOnSceneInfoUpdated;
        }
    }

    private void SetNewHome(Vector2Int newHomeCoordinates)
    {
        homeCoords = newHomeCoordinates;
        UpdateSetHomePanel();
    }

    public void UpdateData(MinimapHUDModel model)
    {
        this.model = model;
        view?.UpdateData(this.model);
    }

    public void UpdateSceneName(string sceneName)
    {
        model.sceneName = sceneName;
        view?.UpdateData(model);
    }

    public void UpdatePlayerPosition(Vector2 position)
    {
        const string format = "{0},{1}";
        UpdatePlayerPosition(string.Format(format, position.x, position.y));
    }

    public void UpdateSetHomePanel()
    {
        view.UpdateSetHomePanel(currentCoords == homeCoords);
    }

    public void UpdatePlayerPosition(string position)
    {
        model.playerPosition = position;
        view?.UpdateData(model);
    }

    public void AddZoomDelta(float delta) { minimapZoom.Set(Mathf.Clamp01(minimapZoom.Get() + delta)); }

    public void ToggleOptions() { view.ToggleOptions(); }

    public void ToggleSceneUI(bool isUIOn) { DataStore.i.HUDs.isSceneUIEnabled.Set(isUIOn); }

    public void AddBookmark()
    {
        //TODO:
        if (VERBOSE)
        {
            Debug.Log("Add bookmark pressed");
        }
    }

    public void ReportScene()
    {
        var coords = playerCoords.Get();
        WebInterface.SendReportScene($"{coords.x},{coords.y}");
    }

    public void SetHomeScene(bool isOn)
    {
        var coords = playerCoords.Get();
        if (playerCoords == homeCoords)
        {
            if (!isOn)
                locationController.SetHomeScene(new Vector2(0,0));
        }
        else
        { 
            if(isOn)
                locationController.SetHomeScene(new Vector2(coords.x,coords.y));
        }
    }

    public void ChangeVisibilityForBuilderInWorld(bool current, bool previus) { view.gameObject.SetActive(current); }

    public void SetVisibility(bool visible) { view.SetVisibility(visible); }

    /// <summary>
    /// Enable user's around button/indicator that shows the amount of users around player
    /// and toggle the list of players' visibility when pressed
    /// </summary>
    /// <param name="controller">Controller for the players' list HUD</param>
    public void AddUsersAroundIndicator(UsersAroundListHUDController controller)
    {
        view.usersAroundListHudButton.gameObject.SetActive(true);
        controller.SetButtonView(view.usersAroundListHudButton);
        controller.ToggleUsersCount(false);
        KernelConfig.i.EnsureConfigInitialized().Then(kc => controller.ToggleUsersCount(kc.features.enablePeopleCounter));
    }

    private void OnOnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
    {
        if (sceneInfo.parcels.Contains(CommonScriptableObjects.playerCoords.Get()))
        {
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnOnSceneInfoUpdated;
            UpdateSceneName(sceneInfo.name);
        }
    }
}