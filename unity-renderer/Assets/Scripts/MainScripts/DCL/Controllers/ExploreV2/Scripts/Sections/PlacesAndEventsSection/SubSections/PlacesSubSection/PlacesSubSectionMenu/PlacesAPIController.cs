using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static HotScenesController;

public interface IPlacesAPIController
{
    /// <summary>
    /// Request all places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    void GetAllPlaces(Action<List<HotSceneInfo>> OnCompleted);

    /// <summary>
    /// Request all favorite places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    void GetAllFavorites(Action<List<HotSceneInfo>> OnCompleted);

    /// <summary>
    /// Add a place to favorites
    /// </summary>
    /// <param name="placeUUID">UUID to identify the place.</param>
    void AddFavorite(string placeUUID);

    /// <summary>
    /// Remove a place from favorites
    /// </summary>
    /// <param name="placeUUID">UUID to identify the place.</param>
    void RemoveFavorite(string placeUUID);
}

[ExcludeFromCodeCoverage]
public class PlacesAPIController : IPlacesAPIController
{
    private readonly HotScenesController hotScenesController = HotScenesController.i;

    internal event Action<List<HotSceneInfo>> OnGetOperationCompleted;

    public void GetAllPlaces(Action<List<HotSceneInfo>> OnCompleted)
    {
        OnGetOperationCompleted += OnCompleted;
        WebInterface.FetchHotScenes();

        hotScenesController.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        hotScenesController.OnHotSceneListFinishUpdating += OnFetchHotScenes;
    }

    public void GetAllFavorites(Action<List<HotSceneInfo>> OnCompleted)
    {
        OnCompleted?.Invoke(new List<HotSceneInfo>());
    }

    public void AddFavorite(string placeUUID)
    {
        throw new NotImplementedException();
    }

    public void RemoveFavorite(string placeUUID)
    {
        throw new NotImplementedException();
    }

    private void OnFetchHotScenes()
    {
        hotScenesController.OnHotSceneListFinishUpdating -= OnFetchHotScenes;

        OnGetOperationCompleted?.Invoke(hotScenesController.hotScenesList);
        OnGetOperationCompleted = null;
    }
}
