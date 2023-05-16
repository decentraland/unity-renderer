using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UnityEngine.Networking;

public interface IPlacesAPIController
{

    /// <summary>
    /// Request all places from the server with pagination support, sorted by most active.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    /// <param name="pageNumber">Number of the page to request.</param>
    /// <param name="amountPerPage">Size of the page.</param>
    UniTask GetAllPlacesFromPlacesAPI(Action<List<IHotScenesController.PlaceInfo>, int> OnCompleted, int pageNumber, int amountPerPage, CancellationToken cancellationToken);

    /// <summary>
    /// Request all favorite places from the server.
    /// </summary>
    /// <param name="OnCompleted">It will be triggered when the operation has finished successfully.</param>
    UniTask GetAllFavorites(Action<List<IHotScenesController.PlaceInfo>> OnCompleted, CancellationToken cancellationToken);

    /// <summary>
    /// Set a place as favorite or not.
    /// </summary>
    /// <param name="placeUUID">UUID of the place to favorite or not.</param>
    /// <param name="isFavorite">bool to set favorite or remove from favorite.</param>
    UniTask SetPlaceFavorite(string placeUUID, bool isFavorite, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
public class PlacesAPIController : IPlacesAPIController
{
    private const string FAVORITE_PLACES_URL = "https://places.decentraland.org/api/places?only_favorites=true&with_realms_detail=true";
    private const string PLACES_URL = "https://places.decentraland.org/api/places?order_by=most_active&order=desc&with_realms_detail=true";
    private const string FAVORITE_SET_URL_START = "https://places.decentraland.org/api/places/";
    private const string FAVORITE_SET_URL_END = "/favorites";
    private Service<IWebRequestController> webRequestController;

    public async UniTask GetAllPlacesFromPlacesAPI(Action<List<IHotScenesController.PlaceInfo>, int> OnCompleted, int offset, int amountPerPage, CancellationToken cancellationToken)
    {
        UnityWebRequest result = await webRequestController.Ref.GetAsync(ComposePlacecsURLWithPage(offset, amountPerPage), isSigned: true, cancellationToken: cancellationToken);
        IHotScenesController.PlacesAPIResponse placesAPIResponse = Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text);
        OnCompleted?.Invoke(placesAPIResponse.data, placesAPIResponse.total);
    }

    public async UniTask GetAllFavorites(Action<List<IHotScenesController.PlaceInfo>> OnCompleted, CancellationToken cancellationToken)
    {
        UnityWebRequest result = await webRequestController.Ref.GetAsync(FAVORITE_PLACES_URL, isSigned: true, cancellationToken: cancellationToken);
        OnCompleted?.Invoke(Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text).data);
    }

    public async UniTask SetPlaceFavorite(string placeUUID, bool isFavorite, CancellationToken cancellationToken)
    {
        string payload = "{\"favorites\":"+isFavorite.ToString().ToLower()+"}";
        await webRequestController.Ref.PatchAsync(ComposeAddRemovePlaceUrl(placeUUID), payload, isSigned: true, cancellationToken: cancellationToken);
    }

    private string ComposePlacecsURLWithPage(int offset, int amountPerPage) =>
        $"{PLACES_URL}&offset={offset}&limit={amountPerPage}";

    private string ComposeAddRemovePlaceUrl(string placeId) =>
        FAVORITE_SET_URL_START + placeId + FAVORITE_SET_URL_END;
}
