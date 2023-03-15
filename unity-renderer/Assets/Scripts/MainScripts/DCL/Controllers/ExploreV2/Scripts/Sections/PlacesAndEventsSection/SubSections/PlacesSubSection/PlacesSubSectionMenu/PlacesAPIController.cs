using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Networking;
using static HotScenesController;

public interface IPlacesAPIController
{
    /// <summary>
    /// Request all places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    void GetAllPlaces(Action<List<HotSceneInfo>> OnCompleted);

    UniTask GetAllPlacesFromPlacesAPI(Action<List<PlaceInfo>> OnCompleted);

    /// <summary>
    /// Request all favorite places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    UniTask GetAllFavorites(Action<List<PlaceInfo>> OnCompleted);
}

[ExcludeFromCodeCoverage]
public class PlacesAPIController : IPlacesAPIController
{
    private const string FAVORITE_PLACES_URL = "https://places.decentraland.org/api/places?only_favorites=true";
    private const string PLACES_URL = "https://places.decentraland.org/api/places?order_by=most_active&order=desc&with_realms_detail=true";
    private readonly HotScenesController hotScenesController = HotScenesController.i;
    private Service<IWebRequestController> webRequestController;

    internal event Action<List<HotSceneInfo>> OnGetOperationCompleted;

    public void GetAllPlaces(Action<List<HotSceneInfo>> OnCompleted)
    {
        OnGetOperationCompleted += OnCompleted;
        WebInterface.FetchHotScenes();

        hotScenesController.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        hotScenesController.OnHotSceneListFinishUpdating += OnFetchHotScenes;
    }

    public async UniTask GetAllPlacesFromPlacesAPI(Action<List<PlaceInfo>> OnCompleted)
    {
        UnityWebRequest result = await webRequestController.Ref.GetAsync(PLACES_URL, isSigned: true);
        Debug.Log(result.downloadHandler.text);
        List<PlaceInfo> placeInfos = Utils.SafeFromJson<PlacesAPIResponse>(result.downloadHandler.text).data;
        Debug.Log(placeInfos[0].realms_detail.Length);
        OnCompleted?.Invoke(placeInfos);
    }

    public async UniTask GetAllFavorites(Action<List<PlaceInfo>> OnCompleted)
    {
        UnityWebRequest result = await webRequestController.Ref.GetAsync(FAVORITE_PLACES_URL, isSigned: true);
        OnCompleted?.Invoke(Utils.SafeFromJson<PlacesAPIResponse>(result.downloadHandler.text).data);
    }

    private void OnFetchHotScenes()
    {
        hotScenesController.OnHotSceneListFinishUpdating -= OnFetchHotScenes;

        OnGetOperationCompleted?.Invoke(hotScenesController.hotScenesList);
        OnGetOperationCompleted = null;
    }
}
