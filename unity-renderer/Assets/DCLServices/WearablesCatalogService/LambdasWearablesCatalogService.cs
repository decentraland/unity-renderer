using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLServices.Lambdas;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DCLServices.WearablesCatalogService
{
    /// <summary>
    /// This service implements a direct way of getting wearables sending the requests directly to lambdas.
    /// </summary>
    public class LambdasWearablesCatalogService : IWearablesCatalogService, ILambdaServiceConsumer<WearableResponse>
    {
        public BaseDictionary<string, WearableItem> WearablesCatalog { get; }

        private const string PAGINATED_WEARABLES_END_POINT = "nfts/wearables/";
        private const string NON_PAGINATED_WEARABLES_END_POINT = "collections/wearables/";
        private const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";
        private const int REQUESTS_TIME_OUT_SECONDS = 45;

        private Service<ILambdasService> lambdasService;
        private CancellationTokenSource serviceCts;
        private readonly Dictionary<string, int> wearablesInUseCounters = new ();
        private readonly Dictionary<string, LambdaResponsePagePointer<WearableResponse>> ownerWearablesPagePointers = new ();
        private readonly Dictionary<(string, string), LambdaResponsePagePointer<WearableResponse>> thirdPartyCollectionPagePointers = new ();
        private readonly List<string> pendingWearablesToRequest = new ();
        private UniTaskCompletionSource<IReadOnlyList<WearableItem>> lastRequestSource;

        public LambdasWearablesCatalogService(BaseDictionary<string, WearableItem> wearablesCatalog)
        {
            WearablesCatalog = wearablesCatalog;
        }

        public void Initialize()
        {
            serviceCts = serviceCts.SafeRestart();
        }

        public void Dispose()
        {
            serviceCts.SafeCancelAndDispose();
            serviceCts = null;
            Clear();
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {
            var createNewPointer = false;
            if (!ownerWearablesPagePointers.TryGetValue(userId, out var pagePointer))
            {
                createNewPointer = true;
            }
            else if (cleanCachedPages)
            {
                pagePointer.Dispose();
                ownerWearablesPagePointers.Remove(userId);
                createNewPointer = true;
            }

            if (createNewPointer)
            {
                ownerWearablesPagePointers[userId] = pagePointer = new LambdaResponsePagePointer<WearableResponse>(
                    PAGINATED_WEARABLES_END_POINT + userId,
                    pageSize, ct, this);
            }

            var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);

            if (!pageResponse.success)
                throw new Exception($"The request of the owned wearables for '{userId}' failed!");

            AddWearablesToCatalog(pageResponse.response.wearables);

            return pageResponse.response.wearables;
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct)
        {
            var serviceResponse = await lambdasService.Ref.Get<WearableResponse>(
                NON_PAGINATED_WEARABLES_END_POINT,
                NON_PAGINATED_WEARABLES_END_POINT,
                REQUESTS_TIME_OUT_SECONDS,
                urlEncodedParams: ("collectionId", BASE_WEARABLES_COLLECTION_ID),
                cancellationToken: ct);

            if (!serviceResponse.success)
                throw new Exception("The request of the base wearables failed!");

            AddWearablesToCatalog(serviceResponse.response.wearables);

            return serviceResponse.response.wearables;
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {
            var createNewPointer = false;
            if (!thirdPartyCollectionPagePointers.TryGetValue((userId, collectionId), out var pagePointer))
            {
                createNewPointer = true;
            }
            else if (cleanCachedPages)
            {
                pagePointer.Dispose();
                thirdPartyCollectionPagePointers.Remove((userId, collectionId));
                createNewPointer = true;
            }

            if (createNewPointer)
            {
                thirdPartyCollectionPagePointers[(userId, collectionId)] = pagePointer = new LambdaResponsePagePointer<WearableResponse>(
                    PAGINATED_WEARABLES_END_POINT + $"{userId}?collectionId={collectionId}",
                    pageSize, ct, this);
            }

            var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);

            if (!pageResponse.success)
                throw new Exception($"The request of the '{collectionId}' third party wearables collection of '{userId}' failed!");

            AddWearablesToCatalog(pageResponse.response.wearables);

            return pageResponse.response.wearables;
        }

        public async UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct)
        {
            if (WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                if (wearablesInUseCounters.ContainsKey(wearableId))
                    wearablesInUseCounters[wearableId]++;

                return wearable;
            }

            ct.ThrowIfCancellationRequested();

            try
            {
                // All the requests happened during the same frames interval are sent together
                return await SyncWearablesRequestsAsync(wearableId, ct);
            }
            catch (OperationCanceledException) { return null; }
        }

        public void AddWearablesToCatalog(IEnumerable<WearableItem> wearableItems)
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

                if (wearablesInUseCounters[wearableToRemove] <= 0)
                    continue;

                WearablesCatalog.Remove(wearableToRemove);
                wearablesInUseCounters.Remove(wearableToRemove);
            }
        }

        public void EmbedWearables(IEnumerable<WearableItem> wearables)
        {
            foreach (WearableItem wearableItem in wearables)
            {
                WearablesCatalog[wearableItem.id] = wearableItem;

                if (wearablesInUseCounters.ContainsKey(wearableItem.id))
                    wearablesInUseCounters[wearableItem.id] = int.MaxValue; //A high value to ensure they are not removed
            }
        }

        public void Clear()
        {
            WearablesCatalog.Clear();
            wearablesInUseCounters.Clear();
            pendingWearablesToRequest.Clear();
        }

        public bool IsValidWearable(string wearableId)
        {
            if (!WearablesCatalog.TryGetValue(wearableId, out var wearable))
                return false;

            return wearable != null;
        }

        UniTask<(WearableResponse response, bool success)> ILambdaServiceConsumer<WearableResponse>.CreateRequest
            (string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            lambdasService.Ref.Get<WearableResponse>(
                PAGINATED_WEARABLES_END_POINT,
                endPoint,
                REQUESTS_TIME_OUT_SECONDS,
                ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
                cancellationToken,
                LambdaPaginatedResponseHelper.GetPageSizeParam(pageSize),
                LambdaPaginatedResponseHelper.GetPageNumParam(pageNumber));

        private async UniTask<WearableItem> SyncWearablesRequestsAsync(string newWearableId, CancellationToken ct)
        {
            pendingWearablesToRequest.Add(newWearableId);
            lastRequestSource ??= new UniTaskCompletionSource<IReadOnlyList<WearableItem>>();
            var sourceToAwait = lastRequestSource;

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: ct);

            IReadOnlyList<WearableItem> result;

            if (pendingWearablesToRequest.Count > 0)
            {
                lastRequestSource = null;

                using var wearableIdsPool = PoolUtils.RentList<string>();
                var wearableIds = wearableIdsPool.GetList();
                wearableIds.AddRange(pendingWearablesToRequest);
                pendingWearablesToRequest.Clear();

                (WearableResponse response, bool success) serviceResponse;

                try
                {
                    serviceResponse = await lambdasService.Ref.Get<WearableResponse>(
                        NON_PAGINATED_WEARABLES_END_POINT,
                        NON_PAGINATED_WEARABLES_END_POINT,
                        REQUESTS_TIME_OUT_SECONDS,
                        urlEncodedParams: GetWearablesUrlParams(wearableIds),
                        cancellationToken: serviceCts.Token);
                }
                catch (Exception e)
                {
                    sourceToAwait.TrySetException(e);
                    throw;
                }

                if (!serviceResponse.success)
                {
                    Exception e = new Exception($"The request of the wearables ('{string.Join(", ", wearableIds)}') failed!");
                    sourceToAwait.TrySetException(e);
                    throw e;
                }

                AddWearablesToCatalog(serviceResponse.response.wearables);
                result = serviceResponse.response.wearables;
                sourceToAwait.TrySetResult(result);
            }
            else
                result = await sourceToAwait.Task;

            ct.ThrowIfCancellationRequested();

            return result.FirstOrDefault(x => x.id == newWearableId);
        }

        private static (string paramName, string paramValue)[] GetWearablesUrlParams(IEnumerable<string> wearableIds) =>
            wearableIds.Select(id => ("wearableId", id)).ToArray();
    }
}
