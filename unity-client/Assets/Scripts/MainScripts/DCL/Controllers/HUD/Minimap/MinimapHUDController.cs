using DCL.Interface;
using UnityEngine;

public class MinimapHUDController : IHUD
{
    private static bool VERBOSE = false;

    public MinimapHUDView view;

    private FloatVariable minimapZoom => CommonScriptableObjects.minimapZoom;
    private StringVariable currentSceneId => CommonScriptableObjects.sceneID;

    public MinimapHUDModel model { get; private set; } = new MinimapHUDModel();
    public RectTransform minimapTooltipReference { get => view.minimapTooltipReference; }
    public RectTransform usersAroundTooltipReference { get => view.usersAroundTooltipReference; }

    public MinimapHUDController() : this(new MinimapHUDModel())
    {
    }

    public MinimapHUDController(MinimapHUDModel model)
    {
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange += ChangeVisibilityForBuilderInWorld;
        minimapZoom.Set(1f);

        view = MinimapHUDView.Create(this);
        UpdateData(model);
    }

    public void Dispose()
    {
        if (view != null)
            UnityEngine.Object.Destroy(view.gameObject);

        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChange;
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange -= ChangeVisibilityForBuilderInWorld;
    }

    private void OnPlayerCoordsChange(Vector2Int current, Vector2Int previous)
    {
        UpdatePlayerPosition(current);
        UpdateSceneName(MinimapMetadata.GetMetadata().GetSceneInfo(current.x, current.y)?.name);
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

    public void UpdatePlayerPosition(string position)
    {
        model.playerPosition = position;
        view?.UpdateData(model);
    }

    public void AddZoomDelta(float delta)
    {
        minimapZoom.Set(Mathf.Clamp01(minimapZoom.Get() + delta));
    }

    public void ToggleOptions()
    {
        view.ToggleOptions();
    }

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
        if (!string.IsNullOrEmpty(currentSceneId))
            WebInterface.SendReportScene(currentSceneId);
    }

    public void ChangeVisibilityForBuilderInWorld(bool current, bool previus)
    {
        SetVisibility(current);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    /// <summary>
    /// Enable user's around button/indicator that shows the amount of users around player
    /// and toggle the list of players' visibility when pressed
    /// </summary>
    /// <param name="controller">Controller for the players' list HUD</param>
    public void AddUsersAroundIndicator(UsersAroundListHUDController controller)
    {
        view.usersAroundListHudButton.gameObject.SetActive(true);
        controller.SetButtonView(view.usersAroundListHudButton);
    }
}