using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    public class LambdasWearablesCatalogService : IWearablesCatalogService, ILambdaServiceConsumer<WearableResponse>
    {
        public BaseDictionary<string, WearableItem> WearablesCatalog { get; }

        private const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";
        private const string WEARABLES_BY_ID_END_POINT = "collections/wearables?wearableId={wearableId}/";
        private const string WEARABLES_BY_OWNER_END_POINT = "nfts/wearables/{userId}/";
        private const string WEARABLES_BY_COLLECTION_END_POINT = "collections/wearables?collectionId={collectionId}/";
        private const string WEARABLES_BY_THIRD_PARTY_COLLECTION_END_POINT = "nfts/thirpartywearables/{userId}?collectionId={collectionId}/";
        private const int REQUESTS_TIME_OUT_SECONDS = 45;
        private const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;
        private const int TIME_TO_CHECK_FOR_UNUSED_WEARABLES = 10;

        private Service<ILambdasService> lambdasService;
        private readonly Dictionary<string, int> wearablesInUseCounters = new ();
        private readonly Dictionary<string, LambdaResponsePagePointer<WearableResponse>> wearablesByIdPagePointers = new ();
        private readonly Dictionary<string, LambdaResponsePagePointer<WearableResponse>> wearablesByCollectionPagePointers = new ();
        private readonly Dictionary<string, LambdaResponsePagePointer<WearableResponse>> wearablesByOwnerPagePointers = new ();
        private readonly Dictionary<(string, string), LambdaResponsePagePointer<WearableResponse>> wearablesByThirdPartyCollectionPagePointers = new ();
        private CancellationTokenSource checkForUnusedWearablesCts;

        public LambdasWearablesCatalogService(BaseDictionary<string, WearableItem> wearablesCatalog)
        {
            WearablesCatalog = wearablesCatalog;
        }

        public void Initialize()
        {
            // Check unused wearables (to be removed from our catalog) only every [TIME_TO_CHECK_FOR_UNUSED_WEARABLES] seconds
            checkForUnusedWearablesCts = new ();
            CheckForUnusedWearablesAsync(checkForUnusedWearablesCts.Token).Forget();
        }

        public void Dispose()
        {
            checkForUnusedWearablesCts?.Cancel();
            checkForUnusedWearablesCts?.Dispose();
            checkForUnusedWearablesCts = null;
            Clear();
        }

        public async UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct)
        {
            if (WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                wearablesInUseCounters[wearableId]++;
                return wearable;
            }

            if (!wearablesByIdPagePointers.ContainsKey(wearableId))
            {
                wearablesByIdPagePointers[wearableId] = new (
                    WEARABLES_BY_ID_END_POINT.Replace("{wearableId}", wearableId),
                    1, ct, this);
            }

            var pageResponse = await wearablesByIdPagePointers[wearableId].GetPageAsync(1, ct);
            AddWearablesToCatalog(pageResponse.response.wearables.ToArray());

            return pageResponse.response.wearables.Count > 0 ? pageResponse.response.wearables[0] : null;
        }

        public async UniTask<WearableItem[]> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
        {
            if (!wearablesByOwnerPagePointers.ContainsKey(userId))
            {
                wearablesByOwnerPagePointers[userId] = new (
                    WEARABLES_BY_OWNER_END_POINT.Replace("{userId}", userId),
                    pageSize, ct, this);
            }

            var pageResponse = await wearablesByOwnerPagePointers[userId].GetPageAsync(pageNumber, ct);
            AddWearablesToCatalog(pageResponse.response.wearables.ToArray());

            return pageResponse.response.wearables.ToArray();
        }

        public async UniTask<WearableItem[]> RequestBaseWearablesAsync(CancellationToken ct)
        {
            if (!wearablesByCollectionPagePointers.ContainsKey(BASE_WEARABLES_COLLECTION_ID))
            {
                wearablesByCollectionPagePointers[BASE_WEARABLES_COLLECTION_ID] = new (
                    WEARABLES_BY_COLLECTION_END_POINT.Replace("{collectionId}", BASE_WEARABLES_COLLECTION_ID),
                    1, ct, this);
            }

            var pageResponse = await wearablesByCollectionPagePointers[BASE_WEARABLES_COLLECTION_ID].GetPageAsync(1, ct);
            AddWearablesToCatalog(pageResponse.response.wearables.ToArray());

            return pageResponse.response.wearables.ToArray();
        }

        public async UniTask<WearableItem[]> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, CancellationToken ct)
        {
            if (!wearablesByThirdPartyCollectionPagePointers.ContainsKey((userId, collectionId)))
            {
                wearablesByThirdPartyCollectionPagePointers[(userId, collectionId)] = new (
                    WEARABLES_BY_THIRD_PARTY_COLLECTION_END_POINT
                       .Replace($"{userId}", userId)
                       .Replace("{collectionId}", collectionId),
                    1, ct, this);
            }

            var pageResponse = await wearablesByThirdPartyCollectionPagePointers[(userId, collectionId)].GetPageAsync(1, ct);
            AddWearablesToCatalog(pageResponse.response.wearables.ToArray());

            return pageResponse.response.wearables.ToArray();
        }

        public void AddWearablesToCatalog(WearableItem[] wearableItems)
        {
            foreach (WearableItem wearableItem in wearableItems)
            {
                if (WearablesCatalog.ContainsKey(wearableItem.id))
                    continue;

                wearableItem.SanitizeHidesLists();
                WearablesCatalog.Add(wearableItem.id, wearableItem);

                if (!wearablesInUseCounters.ContainsKey(wearableItem.id))
                    wearablesInUseCounters.Add(wearableItem.id, 1);
            }
        }

        public void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds)
        {
            foreach (string wearableId in wearableIds)
            {
                WearablesCatalog.Remove(wearableId);
                wearablesInUseCounters.Remove(wearableId);
            }
        }

        public void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove)
        {
            foreach (string wearableToRemove in wearablesInUseToRemove)
            {
                if (!wearablesInUseCounters.ContainsKey(wearableToRemove))
                    continue;

                wearablesInUseCounters[wearableToRemove]--;
            }
        }

        public void EmbedWearables(IEnumerable<WearableItem> wearables)
        {
            foreach (WearableItem wearableItem in wearables)
            {
                WearablesCatalog[wearableItem.id] = wearableItem;
                wearablesInUseCounters[wearableItem.id] = 10000; //A high value to ensure they are not removed
            }
        }

        public void Clear()
        {
            WearablesCatalog.Clear();
            wearablesInUseCounters.Clear();
        }

        UniTask<(WearableResponse response, bool success)> ILambdaServiceConsumer<WearableResponse>.CreateRequest
            (string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            lambdasService.Ref.Get<WearableResponse>(
                "",
                endPoint,
                REQUESTS_TIME_OUT_SECONDS,
                ATTEMPTS_NUMBER,
                cancellationToken,
                LambdaPaginatedResponseHelper.GetPageSizeParam(pageSize),
                LambdaPaginatedResponseHelper.GetPageNumParam(pageNumber));

        private async UniTaskVoid CheckForUnusedWearablesAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(TIME_TO_CHECK_FOR_UNUSED_WEARABLES), cancellationToken: ct);

                if (wearablesInUseCounters.Count <= 0)
                    continue;

                List<string> wearablesToDestroy = new ();
                foreach (var wearableInUse in wearablesInUseCounters)
                {
                    if (wearableInUse.Value <= 0)
                    {
                        wearablesToDestroy.Add(wearableInUse.Key);
                    }
                }

                foreach (string wearableToDestroy in wearablesToDestroy)
                {
                    WearablesCatalog.Remove(wearableToDestroy);
                    wearablesInUseCounters.Remove(wearableToDestroy);
                }
            }
        }
    }
}
