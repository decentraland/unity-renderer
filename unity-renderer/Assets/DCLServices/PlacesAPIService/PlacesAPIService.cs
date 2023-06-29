using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLServices.Lambdas;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.PlacesAPIService
{
    public interface IPlacesAPIService: IService
    {
        UniTask<(IReadOnlyList<IHotScenesController.PlaceInfo> places, int total)> GetMostActivePlaces(int pageNumber, int pageSize, CancellationToken ct, bool renewCache = false);

        UniTask<IHotScenesController.PlaceInfo> GetPlace(Vector2Int coords, CancellationToken ct, bool renewCache = false);

        UniTask<IHotScenesController.PlaceInfo> GetPlace(string placeUUID, CancellationToken ct, bool renewCache = false);

        UniTask<IReadOnlyList<IHotScenesController.PlaceInfo>> GetFavorites(CancellationToken ct, bool renewCache = false);

        UniTask SetPlaceFavorite(string placeUUID, bool isFavorite, CancellationToken ct);
        UniTask SetPlaceFavorite(Vector2Int coords, bool isFavorite, CancellationToken ct);

        UniTask<bool> IsFavoritePlace(IHotScenesController.PlaceInfo placeInfo, CancellationToken ct, bool renewCache = false);
        UniTask<bool> IsFavoritePlace(Vector2Int coords, CancellationToken ct, bool renewCache = false);
        UniTask<bool> IsFavoritePlace(string placeUUID, CancellationToken ct, bool renewCache = false);
    }

    public class PlacesAPIService : IPlacesAPIService, ILambdaServiceConsumer<IHotScenesController.PlacesAPIResponse>
    {
        private readonly IPlacesAPIClient client;

        internal readonly Dictionary<int, LambdaResponsePagePointer<IHotScenesController.PlacesAPIResponse>> activePlacesPagePointers = new ();
        internal readonly Dictionary<string, IHotScenesController.PlaceInfo> placesById = new ();
        internal readonly Dictionary<Vector2Int, IHotScenesController.PlaceInfo> placesByCoords = new ();

        //Favorites
        internal bool composedFavoritesDirty = true;
        internal readonly List<IHotScenesController.PlaceInfo> composedFavorites = new ();
        internal UniTaskCompletionSource<List<IHotScenesController.PlaceInfo>> serverFavoritesCompletionSource = null;
        private DateTime serverFavoritesLastRetrieval = DateTime.MinValue;
        internal readonly Dictionary<string, bool> localFavorites = new ();

        private readonly CancellationTokenSource disposeCts = new ();

        public PlacesAPIService(IPlacesAPIClient client)
        {
            this.client = client;
        }
        public void Initialize() { }

        public async UniTask<(IReadOnlyList<IHotScenesController.PlaceInfo> places, int total)> GetMostActivePlaces(int pageNumber, int pageSize, CancellationToken ct, bool renewCache = false)
        {
            var createNewPointer = false;

            if (!activePlacesPagePointers.TryGetValue(pageSize, out var pagePointer)) { createNewPointer = true; }
            else if (renewCache)
            {
                pagePointer.Dispose();
                activePlacesPagePointers.Remove(pageSize);
                createNewPointer = true;
            }

            if (createNewPointer)
            {
                activePlacesPagePointers[pageSize] = pagePointer = new LambdaResponsePagePointer<IHotScenesController.PlacesAPIResponse>(
                    $"", // not needed, the consumer will compose the URL
                    pageSize, disposeCts.Token, this, TimeSpan.FromSeconds(30));
            }

            (IHotScenesController.PlacesAPIResponse response, bool _) = await pagePointer.GetPageAsync(pageNumber, ct);

            foreach (IHotScenesController.PlaceInfo place in response.data)
            {
                CachePlace(place);
            }

            return (response.data, response.total);
        }

        public async UniTask<IHotScenesController.PlaceInfo> GetPlace(Vector2Int coords, CancellationToken ct, bool renewCache = false)
        {
            if (renewCache)
                placesByCoords.Remove(coords);
            else if (placesByCoords.TryGetValue(coords, out var placeInfo))
                return placeInfo;

            var place = await client.GetPlace(coords, ct);
            CachePlace(place);
            return place;
        }

        public async UniTask<IHotScenesController.PlaceInfo> GetPlace(string placeUUID, CancellationToken ct, bool renewCache = false)
        {
            if (renewCache)
                placesById.Remove(placeUUID);
            else if (placesById.TryGetValue(placeUUID, out var placeInfo))
                return placeInfo;

            var place = await client.GetPlace(placeUUID, ct);
            CachePlace(place);
            return place;
        }

        public async UniTask<IReadOnlyList<IHotScenesController.PlaceInfo>> GetFavorites(CancellationToken ct, bool renewCache = false)
        {
            const int CACHE_EXPIRATION = 30; // Seconds

            // We need to pass the source to avoid conflicts with parallel calls forcing renewCache
            async UniTask RetrieveFavorites(UniTaskCompletionSource<List<IHotScenesController.PlaceInfo>> source)
            {
                // We dont use the ct param, otherwise the whole flow would be cancel if the first call is cancelled
                var favorites = await client.GetFavorites(disposeCts.Token);
                foreach (IHotScenesController.PlaceInfo place in favorites)
                {
                    CachePlace(place);
                }
                composedFavoritesDirty = true;
                source.TrySetResult(favorites);
            }

            if (serverFavoritesCompletionSource == null || renewCache || (DateTime.Now - serverFavoritesLastRetrieval) > TimeSpan.FromSeconds(CACHE_EXPIRATION))
            {
                localFavorites.Clear();
                serverFavoritesLastRetrieval = DateTime.Now;
                serverFavoritesCompletionSource = new UniTaskCompletionSource<List<IHotScenesController.PlaceInfo>>();
                RetrieveFavorites(serverFavoritesCompletionSource).Forget();
            }

            List<IHotScenesController.PlaceInfo> serverFavorites = await serverFavoritesCompletionSource.Task.AttachExternalCancellation(ct);

            if (!composedFavoritesDirty)
                return composedFavorites;

            composedFavorites.Clear();
            foreach (IHotScenesController.PlaceInfo serverFavorite in serverFavorites)
            {
                //skip if it's already in the local favorites cache, it will be added (or not) later
                if(localFavorites.ContainsKey(serverFavorite.id))
                    continue;
                composedFavorites.Add(serverFavorite);
            }

            foreach ((string placeUUID, bool isFavorite) in localFavorites)
            {
                if (!isFavorite)
                    continue;

                if(placesById.TryGetValue(placeUUID, out var place))
                    composedFavorites.Add(place);
            }
            composedFavoritesDirty = false;

            return composedFavorites;
        }

        public async UniTask SetPlaceFavorite(string placeUUID, bool isFavorite, CancellationToken ct)
        {
            localFavorites[placeUUID] = isFavorite;
            composedFavoritesDirty = true;
            await client.SetPlaceFavorite(placeUUID, isFavorite, ct);
        }

        public async UniTask SetPlaceFavorite(Vector2Int coords, bool isFavorite, CancellationToken ct)
        {
            var place = await GetPlace(coords, ct);
            await SetPlaceFavorite(place.id, isFavorite, ct);
        }

        public async UniTask<bool> IsFavoritePlace(IHotScenesController.PlaceInfo placeInfo, CancellationToken ct, bool renewCache = false)
        {
            var favorites = await GetFavorites(ct, renewCache);

            foreach (IHotScenesController.PlaceInfo favorite in favorites)
            {
                if (favorite.id == placeInfo.id)
                    return true;
            }

            return false;
        }

        public async UniTask<bool> IsFavoritePlace(Vector2Int coords, CancellationToken ct, bool renewCache = false)
        {
            // We could call IsFavoritePlace with the placeInfo and avoid code repetition, but this way we can have the calls in parallel
            (IHotScenesController.PlaceInfo placeInfo, IReadOnlyList<IHotScenesController.PlaceInfo> favorites) = await UniTask.WhenAll(GetPlace(coords, ct, renewCache), GetFavorites(ct, renewCache));

            foreach (IHotScenesController.PlaceInfo favorite in favorites)
            {
                if (favorite.id == placeInfo.id)
                    return true;
            }

            return false;
        }

        public async UniTask<bool> IsFavoritePlace(string placeUUID, CancellationToken ct, bool renewCache = false)
        {
            // We could call IsFavoritePlace with the placeInfo and avoid code repetition, but this way we can have the calls in parallel
            (IHotScenesController.PlaceInfo placeInfo, IReadOnlyList<IHotScenesController.PlaceInfo> favorites) = await UniTask.WhenAll(GetPlace(placeUUID, ct, renewCache), GetFavorites(ct, renewCache));

            foreach (IHotScenesController.PlaceInfo favorite in favorites)
            {
                if (favorite.id == placeInfo.id)
                    return true;
            }

            return false;
        }

        internal void CachePlace(IHotScenesController.PlaceInfo placeInfo)
        {
            placesById[placeInfo.id] = placeInfo;

            foreach (Vector2Int placeInfoPosition in placeInfo.Positions) { placesByCoords[placeInfoPosition] = placeInfo; }
        }

        public async UniTask<(IHotScenesController.PlacesAPIResponse response, bool success)> CreateRequest(string endPoint, int pageSize, int pageNumber, CancellationToken ct)
        {
            var response = await client.GetMostActivePlaces(pageNumber, pageSize, ct);
            // Client will handle most of the error handling and throw if needed
            return (response, true);
        }

        public void Dispose()
        {
            disposeCts.SafeCancelAndDispose();
        }
    }
}
