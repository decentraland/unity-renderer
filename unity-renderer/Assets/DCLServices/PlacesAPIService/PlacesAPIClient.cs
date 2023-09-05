using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
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
        UniTask<IHotScenesController.PlacesAPIResponse> GetMostActivePlaces(int pageNumber, int pageSize, string filter = "", string sort = "", CancellationToken ct = default);
        UniTask<IHotScenesController.PlaceInfo> GetPlace(Vector2Int coords, CancellationToken ct);

        UniTask<IHotScenesController.PlaceInfo> GetPlace(string placeUUID, CancellationToken ct);

        UniTask<List<IHotScenesController.PlaceInfo>> GetPlacesByCoordsList(List<Vector2Int> coordsList, CancellationToken ct);

        UniTask<List<IHotScenesController.PlaceInfo>> GetFavorites(CancellationToken ct);

        UniTask SetPlaceFavorite(string placeUUID, bool isFavorite, CancellationToken ct);
        UniTask SetPlaceVote(bool? isUpvote, string placeUUID, CancellationToken ct);
        UniTask<List<string>> GetPointsOfInterestCoords(CancellationToken ct);
    }

    public class PlacesAPIClient: IPlacesAPIClient
    {
        private const string BASE_URL = "https://places.decentraland.org/api/places";
        private const string BASE_URL_ZONE = "https://places.decentraland.zone/api/places";
        private const string POI_URL = "https://dcl-lists.decentraland.org/pois";
        private readonly IWebRequestController webRequestController;

        public PlacesAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<IHotScenesController.PlacesAPIResponse> SearchPlaces(string searchString, int pageNumber, int pageSize, CancellationToken ct)
        {
            const string URL = BASE_URL + "?with_realms_detail=true&search={0}&offset={1}&limit={2}";
            var result = await webRequestController.GetAsync(string.Format(URL, searchString.Replace(" ", "+"), pageNumber * pageSize, pageSize), cancellationToken: ct, isSigned: true);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching most active places info:\n{result.error}");

            var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing place info:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No place info retrieved:\n{result.downloadHandler.text}");

            return response;
        }

        public async UniTask<IHotScenesController.PlacesAPIResponse> GetMostActivePlaces(int pageNumber, int pageSize, string filter = "", string sort = "", CancellationToken ct = default)
        {
            const string URL = BASE_URL + "?order_by={3}&order=desc&with_realms_detail=true&offset={0}&limit={1}&{2}";
            var result = await webRequestController.GetAsync(string.Format(URL, pageNumber * pageSize, pageSize, filter, sort), cancellationToken: ct, isSigned: true);

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
            var result = await webRequestController.GetAsync(string.Format(URL, coords.x, coords.y), cancellationToken: ct, isSigned: true);

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
            var result = await webRequestController.GetAsync(url, cancellationToken: ct, isSigned: true);

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

        public async UniTask<List<IHotScenesController.PlaceInfo>> GetPlacesByCoordsList(List<Vector2Int> coordsList, CancellationToken ct)
        {
            if (coordsList.Count == 0)
                return new List<IHotScenesController.PlaceInfo>();

            var url = string.Concat(BASE_URL, "?");
            foreach (Vector2Int coords in coordsList)
                url = string.Concat(url, $"positions={coords.x},{coords.y}&with_realms_detail=true&");

            var result = await webRequestController.GetAsync(url, cancellationToken: ct, isSigned: true);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching places info:\n{result.error}");

            var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing places info:\n{result.downloadHandler.text}");

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

        public async UniTask SetPlaceVote(bool? isUpvote, string placeUUID, CancellationToken ct)
        {
            const string URL = BASE_URL + "/{0}/likes";
            const string LIKE_PAYLOAD = "{\"like\": true}";
            const string DISLIKE_PAYLOAD = "{\"like\": false}";
            const string NO_LIKE_PAYLOAD = "{\"like\": null}";
            string payload;

            if (isUpvote == null)
                payload = NO_LIKE_PAYLOAD;
            else
                payload = isUpvote == true ? LIKE_PAYLOAD : DISLIKE_PAYLOAD;

            var result = await webRequestController.PatchAsync(string.Format(URL, placeUUID), payload, isSigned: true, cancellationToken: ct);
            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching place info:\n{result.error}");
        }

        public async UniTask<List<string>> GetPointsOfInterestCoords(CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.PostAsync(POI_URL, "", isSigned: false, cancellationToken: ct);
            var response = Utils.SafeFromJson<PointsOfInterestCoordsAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing get POIs response:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No POIs info retrieved:\n{result.downloadHandler.text}");

            return response.data;
        }
    }
}
