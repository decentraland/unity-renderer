using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Networking;
using static HotScenesController;

public interface IPlacesAPIController
{
    /// <summary>
    /// Request all places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    void GetAllPlaces(Action<List<HotSceneInfo>> OnCompleted);

    UniTask GetAllPlacesFromPlacesAPI(Action<List<PlaceInfo>, int> OnCompleted, int pageNumber, int amountPerPage);

    /// <summary>
    /// Request all favorite places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    UniTask GetAllFavorites(Action<List<PlaceInfo>> OnCompleted);

    UniTask SetPlaceFavorite(string placeUUID, bool isFavorite);
}

[ExcludeFromCodeCoverage]
public class PlacesAPIController : IPlacesAPIController
{
    private const string FAVORITE_PLACES_URL = "https://places.decentraland.org/api/places?only_favorites=true";
    private const string PLACES_URL = "https://places.decentraland.org/api/places?order_by=most_active&order=desc&with_realms_detail=true";
    private const string FAVORITE_SET_URL_START = "https://places.decentraland.org/api/places/";
    private const string FAVORITE_SET_URL_END = "/favorites";
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

    public async UniTask GetAllPlacesFromPlacesAPI(Action<List<PlaceInfo>, int> OnCompleted, int offset, int amountPerPage)
    {
        UnityWebRequest result = await webRequestController.Ref.GetAsync(ComposePlacecsURLWithPage(offset, amountPerPage), isSigned: true);
        PlacesAPIResponse placesAPIResponse = Utils.SafeFromJson<PlacesAPIResponse>(result.downloadHandler.text);
        OnCompleted?.Invoke(placesAPIResponse.data, placesAPIResponse.total);
    }

    public async UniTask GetAllFavorites(Action<List<PlaceInfo>> OnCompleted)
    {
        UnityWebRequest result = await webRequestController.Ref.GetAsync(FAVORITE_PLACES_URL, isSigned: true);
        OnCompleted?.Invoke(Utils.SafeFromJson<PlacesAPIResponse>(result.downloadHandler.text).data);
    }

    public async UniTask SetPlaceFavorite(string placeUUID, bool isFavorite)
    {
        string payload = "{\"favorites\":"+isFavorite.ToString().ToLower()+"}";
        await webRequestController.Ref.PatchAsync(ComposeAddRemovePlaceUrl(placeUUID), payload, isSigned: true);
    }

    private string ComposePlacecsURLWithPage(int offset, int amountPerPage) =>
        $"{PLACES_URL}&offset={offset}&limit={amountPerPage}";

    private string ComposeAddRemovePlaceUrl(string placeId) =>
        FAVORITE_SET_URL_START + placeId + FAVORITE_SET_URL_END;

    private void OnFetchHotScenes()
    {
        hotScenesController.OnHotSceneListFinishUpdating -= OnFetchHotScenes;

        OnGetOperationCompleted?.Invoke(hotScenesController.hotScenesList);
        OnGetOperationCompleted = null;
    }
}
