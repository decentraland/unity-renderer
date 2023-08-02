using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.Lambdas;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.PlacesAPIService
{
    public interface IPlacesAPIClient
    {
        UniTask<IHotScenesController.PlacesAPIResponse> SearchPlaces(string searchString, int pageNumber, int pageSize, CancellationToken ct);
        UniTask<IHotScenesController.PlacesAPIResponse> GetMostActivePlaces(int pageNumber, int pageSize, CancellationToken ct);
        UniTask<IHotScenesController.PlaceInfo> GetPlace(Vector2Int coords, CancellationToken ct);

        UniTask<IHotScenesController.PlaceInfo> GetPlace(string placeUUID, CancellationToken ct);

        UniTask<List<IHotScenesController.PlaceInfo>> GetFavorites(CancellationToken ct);

        UniTask SetPlaceFavorite(string placeUUID, bool isFavorite, CancellationToken ct);
    }

    public class PlacesAPIClient: IPlacesAPIClient
    {
        private const string BASE_URL = "https://places.decentraland.org/api/places";
        private const string BASE_URL_ZONE = "https://places.decentraland.zone/api/places";
        private readonly IWebRequestController webRequestController;

        public PlacesAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<IHotScenesController.PlacesAPIResponse> SearchPlaces(string searchString, int pageNumber, int pageSize, CancellationToken ct)
        {
            const string URL = BASE_URL + "?with_realms_detail=true&search={0}&offset={1}&limit={2}";
            var result = await webRequestController.GetAsync(string.Format(URL, searchString.Replace(" ", "+"), pageNumber * pageSize, pageSize), cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching most active places info:\n{result.error}");

            var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing place info:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No place info retrieved:\n{result.downloadHandler.text}");

            return response;
        }

        public async UniTask<IHotScenesController.PlacesAPIResponse> GetMostActivePlaces(int pageNumber, int pageSize, CancellationToken ct)
        {
            const string URL = BASE_URL + "?order_by=most_active&order=desc&with_realms_detail=true&offset={0}&limit={1}";
            var result = await webRequestController.GetAsync(string.Format(URL, pageNumber * pageSize, pageSize), cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching most active places info:\n{result.error}");

            var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing place info:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No place info retrieved:\n{result.downloadHandler.text}");

            return response;
        }

        public async UniTask<IHotScenesController.PlaceInfo> GetPlace(Vector2Int coords, CancellationToken ct)
        {
            const string URL = BASE_URL + "?positions={0},{1}&with_realms_detail=true";
            var result = await webRequestController.GetAsync(string.Format(URL, coords.x, coords.y), cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching place info:\n{result.error}");

            var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing place info:\n{result.downloadHandler.text}");

            if (response.data.Count == 0)
                throw new NotAPlaceException(coords);

            return response.data[0];
        }

        public async UniTask<IHotScenesController.PlaceInfo> GetPlace(string placeUUID, CancellationToken ct)
        {
            var url = $"{BASE_URL}/{placeUUID}?with_realms_detail=true";
            var result = await webRequestController.GetAsync(url, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching place info:\n{result.error}");

            var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIGetParcelResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing place info:\n{result.downloadHandler.text}");

            if (response.ok == false)
                throw new NotAPlaceException(placeUUID);

            if (response.data == null)
                throw new Exception($"No place info retrieved:\n{result.downloadHandler.text}");

            return response.data;
        }

        public async UniTask<List<IHotScenesController.PlaceInfo>> GetFavorites(CancellationToken ct)
        {
            const string URL = BASE_URL + "?only_favorites=true&with_realms_detail=true";
            UnityWebRequest result = await webRequestController.GetAsync(URL, isSigned: true, cancellationToken: ct);
            var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing get favorites response:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No favorites info retrieved:\n{result.downloadHandler.text}");

            return response.data;
        }

        public async UniTask SetPlaceFavorite(string placeUUID, bool isFavorite, CancellationToken ct)
        {
            const string URL = BASE_URL + "/{0}/favorites";
            const string FAVORITE_PAYLOAD = "{\"favorites\": true}";
            const string NOT_FAVORITE_PAYLOAD = "{\"favorites\": false}";
            var result = await webRequestController.PatchAsync(string.Format(URL, placeUUID), isFavorite ? FAVORITE_PAYLOAD : NOT_FAVORITE_PAYLOAD, isSigned: true, cancellationToken: ct);
            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching place info:\n{result.error}");
        }
    }
}
