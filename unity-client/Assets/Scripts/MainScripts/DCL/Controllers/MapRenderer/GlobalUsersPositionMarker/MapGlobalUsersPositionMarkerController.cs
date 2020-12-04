using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGlobalUsersPositionMarkerController : IDisposable
{
    private const float UPDATE_INTERVAL_INITIAL = 10f;
    private const float UPDATE_INTERVAL_FOREGROUND = 60f;
    private const float UPDATE_INTERVAL_BACKGROUND = 5 * 60f;

    private const int MAX_MARKERS = 100;

    private const int COMMS_RADIUS_THRESHOLD = 2;

    public enum UpdateMode { FOREGROUND, BACKGROUND }

    FetchScenesHandler fetchScenesHandler;
    MarkersHandler markersHandler;
    UserPositionHandler userPositionHandler;
    Transform markersContainer;

    int commsRadius = 4;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="markerPrefab">prefab for markers</param>
    /// <param name="overlayContainer">parent for markers</param>
    /// <param name="coordToMapPosFunc">function to transform coords to map position</param>
    public MapGlobalUsersPositionMarkerController(UserMarkerObject markerPrefab, Transform overlayContainer, Func<float, float, Vector3> coordToMapPosFunc)
    {
        fetchScenesHandler = new FetchScenesHandler(UPDATE_INTERVAL_INITIAL, UPDATE_INTERVAL_FOREGROUND, UPDATE_INTERVAL_BACKGROUND);
        markersHandler = new MarkersHandler(markerPrefab, overlayContainer, MAX_MARKERS, coordToMapPosFunc);
        userPositionHandler = new UserPositionHandler();
        markersContainer = overlayContainer;

        fetchScenesHandler.OnScenesFetched += OnScenesFetched;
        userPositionHandler.OnPlayerCoordsChanged += OnPlayerCoordsChanged;
        CommonScriptableObjects.rendererState.OnChange += OnRenderStateChanged;

        KernelConfig.i.EnsureConfigInitialized().Then(config =>
        {
            commsRadius = (int)config.comms.commRadius + COMMS_RADIUS_THRESHOLD;
            OnPlayerCoordsChanged(userPositionHandler.playerCoords);
        });
        OnRenderStateChanged(CommonScriptableObjects.rendererState.Get(), false);
    }

    /// <summary>
    /// Set update mode. Scene's fetch intervals will smaller when updating in FOREGROUND than when updating in BACKGROUND
    /// </summary>
    /// <param name="updateMode">update mode</param>
    public void SetUpdateMode(UpdateMode updateMode)
    {
        fetchScenesHandler.SetUpdateMode(updateMode);
        markersContainer.gameObject.SetActive(updateMode == UpdateMode.FOREGROUND);
    }

    public void Dispose()
    {
        fetchScenesHandler.OnScenesFetched -= OnScenesFetched;
        userPositionHandler.OnPlayerCoordsChanged -= OnPlayerCoordsChanged;
        CommonScriptableObjects.rendererState.OnChange -= OnRenderStateChanged;

        fetchScenesHandler.Dispose();
        markersHandler.Dispose();
        userPositionHandler.Dispose();
    }

    private void OnScenesFetched(List<HotScenesController.HotSceneInfo> sceneList)
    {
        markersHandler.SetMarkers(sceneList);
    }

    private void OnPlayerCoordsChanged(Vector2Int coords)
    {
        markersHandler.SetExclusionArea(coords, commsRadius);
    }

    private void OnRenderStateChanged(bool current, bool prev)
    {
        if (!current)
            return;

        // NOTE: we start fetching scenes after the renderer is activated for the first time
        CommonScriptableObjects.rendererState.OnChange -= OnRenderStateChanged;
        fetchScenesHandler.Init();
    }
}
