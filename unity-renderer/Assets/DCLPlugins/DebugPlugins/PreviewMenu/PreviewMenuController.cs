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

    private readonly PreviewMenuView menuViewResource;
    private readonly PreviewMenuVisibilityToggleView visibilityToggleViewResource;
    private readonly PreviewMenuPositionView positionViewResource;

    private readonly PreviewMenuView menuView;

    private readonly PreviewMenuVisibilityToggleView showFps;
    private readonly PreviewMenuVisibilityToggleView showBoundingBox;
    private readonly PreviewMenuPositionView positionView;

    public PreviewMenuController()
    {
        menuViewResource = Resources.Load<PreviewMenuView>(MENU_VIEW_RES_PATH);
        menuView = Object.Instantiate(menuViewResource);
        menuView.name = "_PreviewMenu";
        menuView.SetVisible(false);

        visibilityToggleViewResource = Resources.Load<PreviewMenuVisibilityToggleView>(TOGGLEVISIBILITY_VIEW_RES_PATH);
        showFps = Object.Instantiate(visibilityToggleViewResource);
        showFps.SetUp("FPS PANEL", IsFPSpanleOn, OnFPSPanelToggle);

        showBoundingBox = Object.Instantiate(visibilityToggleViewResource);
        showBoundingBox.SetUp("BOUNDING BOXES", IsBoundingBoxOn, OnBoundingBoxToggle);

        positionViewResource = Resources.Load<PreviewMenuPositionView>(POSITION_VIEW_RES_PATH);
        positionView = Object.Instantiate(positionViewResource);

        menuView.AddMenuItem(positionView.transform);
        menuView.AddMenuItem(showFps.transform);
        menuView.AddMenuItem(showBoundingBox.transform);
    }

    public void Dispose()
    {
        Object.Destroy(positionView);
        Object.Destroy(showBoundingBox);
        Object.Destroy(showFps);
        Object.Destroy(menuView);

        Resources.UnloadAsset(menuViewResource);
        Resources.UnloadAsset(visibilityToggleViewResource);
        Resources.UnloadAsset(positionViewResource);
    }

    private static bool IsFPSpanleOn()
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
}