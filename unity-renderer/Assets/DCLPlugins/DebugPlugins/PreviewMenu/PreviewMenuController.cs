using System;
using System.Linq;
using DCL;
using UnityEngine;
using Object = UnityEngine.Object;

internal class PreviewMenuController : IDisposable
{
    internal const string MENU_VIEW_RES_PATH = "PreviewMenuView";
    internal const string TOGGLEVISIBILITY_VIEW_RES_PATH = "PreviewMenuVisibilityToggle";
    internal const string POSITION_VIEW_RES_PATH = "PreviewMenuPositionView";

    internal readonly PreviewMenuView menuView;

    internal readonly PreviewMenuVisibilityToggleView showFps;
    internal readonly PreviewMenuVisibilityToggleView showBoundingBox;
    internal readonly PreviewMenuPositionView positionView;
    internal readonly PreviewMenuVisibilityToggleView spawnPoints;

    public PreviewMenuController()
    {
        var menuViewResource = Resources.Load<PreviewMenuView>(MENU_VIEW_RES_PATH);
        menuView = Object.Instantiate(menuViewResource);
        menuView.name = "_PreviewMenu";
        menuView.SetVisible(false);

        var visibilityToggleViewResource = Resources.Load<PreviewMenuVisibilityToggleView>(TOGGLEVISIBILITY_VIEW_RES_PATH);
        showFps = Object.Instantiate(visibilityToggleViewResource);
        showFps.SetUp("FPS PANEL", IsFPSPanelOn, OnFPSPanelToggle);

        showBoundingBox = Object.Instantiate(visibilityToggleViewResource);
        showBoundingBox.SetUp("BOUNDING BOXES", IsBoundingBoxOn, OnBoundingBoxToggle);

        spawnPoints = Object.Instantiate(visibilityToggleViewResource);
        spawnPoints.SetUp("SPAWN POINTS", IsSpawnPointsOn, OnSpawnPointsToggle);

        var positionViewResource = Resources.Load<PreviewMenuPositionView>(POSITION_VIEW_RES_PATH);
        positionView = Object.Instantiate(positionViewResource);

        menuView.AddMenuItem(positionView.transform);
        menuView.AddMenuItem(showFps.transform);
        menuView.AddMenuItem(showBoundingBox.transform);
        menuView.AddMenuItem(spawnPoints.transform);
    }

    public void Dispose()
    {
        positionView.Dispose();
        showBoundingBox.Dispose();
        showFps.Dispose();
        menuView.Dispose();
        spawnPoints.Dispose();
    }

    private static bool IsFPSPanelOn()
    {
        return DataStore.i.debugConfig.isFPSPanelVisible.Get();
    }

    private static void OnFPSPanelToggle(bool isOn)
    {
        DataStore.i.debugConfig.isFPSPanelVisible.Set(isOn);
    }

    private static bool IsBoundingBoxOn()
    {
        return DataStore.i.debugConfig.showSceneBoundingBoxes.Get()
                        .Any(pair => pair.Value);
    }

    private static void OnBoundingBoxToggle(bool isOn)
    {
        var sceneIds = DataStore.i.debugConfig.showSceneBoundingBoxes.Get()
                                .Select(pair => pair.Key)
                                .ToArray();

        foreach (var sceneId in sceneIds)
        {
            DataStore.i.debugConfig.showSceneBoundingBoxes.AddOrSet(sceneId, isOn);
        }
    }

    private static bool IsSpawnPointsOn()
    {
        return DataStore.i.debugConfig.showSceneSpawnPoints.Get()
                        .Any(pair => pair.Value.enabled.HasValue && pair.Value.enabled.Value);
    }

    private static void OnSpawnPointsToggle(bool isOn)
    {
        var sceneIds = DataStore.i.debugConfig.showSceneSpawnPoints.Get()
                                .Select(pair => pair.Key)
                                .ToArray();

        foreach (var sceneId in sceneIds)
        {
            var data = DataStore.i.debugConfig.showSceneSpawnPoints.Get(sceneId);
            data.enabled = isOn;
            DataStore.i.debugConfig.showSceneSpawnPoints.AddOrSet(sceneId, data);
        }
    }
}