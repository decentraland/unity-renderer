using System;
using System.ComponentModel;
using UnityEngine;

public class MinimapHUDController : IDisposable, IHUD
{
    private MinimapHUDView view;
    public MinimapHUDModel model { get; private set; }

    public MinimapHUDController() : this(new MinimapHUDModel()) { }

    public MinimapHUDController(MinimapHUDModel model)
    {
        CommonScriptableObjects.playerCoords.onChange += OnPlayerCoordsChange;

        view = MinimapHUDView.Create(this);
        UpdateData(model);
    }

    public void Dispose()
    {
        CommonScriptableObjects.playerCoords.onChange -= OnPlayerCoordsChange;
    }

    private void OnPlayerCoordsChange(Vector2Int current, Vector2Int previous)
    {
        UpdatePlayerPosition(current);
        UpdateSceneName(MinimapMetadata.GetMetadata().GetTile(current.x, current.y)?.name);
    }

    public void UpdateData(MinimapHUDModel model)
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

    public void UpdatePlayerPosition(string position)
    {
        model.playerPosition = position;
        view.UpdateData(model);
    }

    public void ToggleOptions()
    {
        view.ToggleOptions();
    }

    public void AddBookmark()
    {
        //TODO:
        Debug.Log("Add bookmark pressed");
    }

    public void ReportScene()
    {
        //TODO:
        Debug.Log("Report scene pressed");
    }

    public void SetConfiguration(HUDConfiguration configuration)
    {
        SetActive(configuration.active);
    }

    private void SetActive(bool active)
    {
        view.SetActive(active);
    }
}