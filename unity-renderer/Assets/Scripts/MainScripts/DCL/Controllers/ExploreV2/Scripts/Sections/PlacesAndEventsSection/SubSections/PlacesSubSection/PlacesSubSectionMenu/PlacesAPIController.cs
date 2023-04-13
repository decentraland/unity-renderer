using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Networking;

public interface IPlacesAPIController
{
    /// <summary>
    /// Request all places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    void GetAllPlaces(Action<List<IHotScenesController.HotSceneInfo>> OnCompleted);

    /// <summary>
    /// Request all favorite places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    UniTask GetAllFavorites(Action<List<IHotScenesController.PlaceInfo>> OnCompleted);
}

[ExcludeFromCodeCoverage]
public class PlacesAPIController : IPlacesAPIController
{
    private const string FAVORITE_PLACES_URL = "https://places.decentraland.org/api/places?only_favorites=true";
    private readonly HotScenesController hotScenesController = HotScenesController.i;
    private Service<IWebRequestController> webRequestController;

    internal event Action<List<IHotScenesController.HotSceneInfo>> OnGetOperationCompleted;

    public void GetAllPlaces(Action<List<IHotScenesController.HotSceneInfo>> OnCompleted)
    {
        OnGetOperationCompleted += OnCompleted;
        WebInterface.FetchHotScenes();

        hotScenesController.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        hotScenesController.OnHotSceneListFinishUpdating += OnFetchHotScenes;
    }

    public async UniTask GetAllFavorites(Action<List<IHotScenesController.PlaceInfo>> OnCompleted)
    {
        UnityWebRequest result = await webRequestController.Ref.GetAsync(FAVORITE_PLACES_URL, isSigned: true);
        OnCompleted?.Invoke(Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text).data);
    }

    private void OnFetchHotScenes()
    {
        hotScenesController.OnHotSceneListFinishUpdating -= OnFetchHotScenes;

        OnGetOperationCompleted?.Invoke(hotScenesController.hotScenesList);
        OnGetOperationCompleted = null;
    }
}
