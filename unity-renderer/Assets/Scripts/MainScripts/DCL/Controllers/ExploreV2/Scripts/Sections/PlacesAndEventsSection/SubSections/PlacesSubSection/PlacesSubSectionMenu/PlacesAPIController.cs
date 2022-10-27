using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static HotScenesController;

public interface IPlacesAPIController
{
    /// <summary>
    /// Request all places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    void GetAllPlaces(Action<List<HotSceneInfo>> OnCompleted);
}

[ExcludeFromCodeCoverage]
public class PlacesAPIController : IPlacesAPIController
{
    internal Action<List<HotSceneInfo>> OnGetOperationCompleted;

    public void GetAllPlaces(Action<List<HotSceneInfo>> OnCompleted)
    {
        OnGetOperationCompleted += OnCompleted;
        WebInterface.FetchHotScenes();

        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        HotScenesController.i.OnHotSceneListFinishUpdating += OnFetchHotScenes;
    }

    internal void OnFetchHotScenes()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        
        OnGetOperationCompleted?.Invoke(HotScenesController.i.hotScenesList);
        OnGetOperationCompleted = null;
    }
}