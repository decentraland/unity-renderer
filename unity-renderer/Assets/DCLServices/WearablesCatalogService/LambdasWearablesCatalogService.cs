using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLServices.Lambdas;
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

        private const string WEARABLES_BY_OWNER_END_POINT = "nfts/wearables/{userId}/";
        private const string WEARABLES_BY_COLLECTION_END_POINT = "collections/wearables?collectionId={collectionId}/";
        private const string WEARABLES_BY_THIRD_PARTY_COLLECTION_END_POINT = "nfts/wearables/{userId}?collectionId={collectionId}/";
        private const string WEARABLES_BY_ID_END_POINT = "collections/wearables?{wearableIdsQuery}/";
        private const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";
        private const int REQUESTS_TIME_OUT_SECONDS = 45;
        private const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;
        private const int TIME_TO_CHECK_FOR_UNUSED_WEARABLES = 10;
        private const int FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS = 1;

        private Service<ILambdasService> lambdasService;
        private CancellationTokenSource serviceCts;
        private readonly Dictionary<string, int> wearablesInUseCounters = new ();
        private readonly Dictionary<string, LambdaResponsePagePointer<WearableResponse>> ownerWearablesPagePointers = new ();
        private readonly Dictionary<(string, string), LambdaResponsePagePointer<WearableResponse>> thirdPartyCollectionPagePointers = new ();
        private readonly Dictionary<string, UniTaskCompletionSource<WearableItem>> awaitingWearableTasks = new ();
        private readonly List<string> pendingWearablesToRequest = new ();

        public LambdasWearablesCatalogService(BaseDictionary<string, WearableItem> wearablesCatalog)
        {
            WearablesCatalog = wearablesCatalog;
        }

        public void Initialize()
        {
            serviceCts = serviceCts.SafeRestart();

            try
            {
                // All the requests happened during the same frames interval are sent together
                CheckForSendingPendingRequestsAsync(serviceCts.Token).Forget();

                // Check unused wearables (to be removed from our catalog) only every [TIME_TO_CHECK_FOR_UNUSED_WEARABLES] seconds
                CheckForUnusedWearablesAsync(serviceCts.Token).Forget();
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            serviceCts.SafeCancelAndDispose();
            serviceCts = null;
            Clear();
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
        {
            if (!ownerWearablesPagePointers.TryGetValue(userId, out var pagePointer))
            {
                ownerWearablesPagePointers[userId] = pagePointer = new (
                    WEARABLES_BY_OWNER_END_POINT.Replace("{userId}", userId),
                    pageSize, ct, this);
            }

            var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);
            AddWearablesToCatalog(pageResponse.response.wearables);

            return pageResponse.response.wearables;
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct)
        {
            var serviceResponse = await lambdasService.Ref.Get<WearableResponse>(
                "",
                WEARABLES_BY_COLLECTION_END_POINT.Replace("{collectionId}", BASE_WEARABLES_COLLECTION_ID),
                REQUESTS_TIME_OUT_SECONDS,
                ATTEMPTS_NUMBER,
                ct,
                LambdaPaginatedResponseHelper.GetPageSizeParam(1),
                LambdaPaginatedResponseHelper.GetPageNumParam(1));

            AddWearablesToCatalog(serviceResponse.response.wearables);

            return serviceResponse.response.wearables;
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, CancellationToken ct)
        {
            if (!thirdPartyCollectionPagePointers.TryGetValue((userId, collectionId), out var pagePointer))
            {
                thirdPartyCollectionPagePointers[(userId, collectionId)] = pagePointer = new (
                    WEARABLES_BY_THIRD_PARTY_COLLECTION_END_POINT
                       .Replace("{userId}", userId)
                       .Replace("{collectionId}", collectionId),
                    pageSize, ct, this);
            }

            var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);
            AddWearablesToCatalog(pageResponse.response.wearables);

            return pageResponse.response.wearables;
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestWearablesAsync(string[] wearableIds, CancellationToken ct)
        {
            var serviceResponse = await lambdasService.Ref.Get<WearableResponse>(
                "",
                WEARABLES_BY_ID_END_POINT.Replace("{wearableIdsQuery}", GetWearablesQuery(wearableIds)),
                REQUESTS_TIME_OUT_SECONDS,
                ATTEMPTS_NUMBER,
                ct,
                LambdaPaginatedResponseHelper.GetPageSizeParam(1),
                LambdaPaginatedResponseHelper.GetPageNumParam(1));

            AddWearablesToCatalog(serviceResponse.response.wearables);

            return serviceResponse.response.wearables;
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
                if (!awaitingWearableTasks.TryGetValue(wearableId, out var taskResult))
                {
                    taskResult = new UniTaskCompletionSource<WearableItem>();
                    awaitingWearableTasks.Add(wearableId, taskResult);

                    // We accumulate all the requests during the same frames interval to send them all together
                    pendingWearablesToRequest.Add(wearableId);
                }

                return await taskResult.Task.AttachExternalCancellation(ct);
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
            }
        }

        public void EmbedWearables(IEnumerable<WearableItem> wearables)
        {
            foreach (WearableItem wearableItem in wearables)
            {
                WearablesCatalog[wearableItem.id] = wearableItem;

                if (wearablesInUseCounters.ContainsKey(wearableItem.id))
                    wearablesInUseCounters[wearableItem.id] = 10000; //A high value to ensure they are not removed
            }
        }

        public void Clear()
        {
            WearablesCatalog.Clear();
            wearablesInUseCounters.Clear();
            awaitingWearableTasks.Clear();
            pendingWearablesToRequest.Clear();
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

        private string GetWearablesQuery(IReadOnlyList<string> wearableIds) =>
            string.Join("&wearableId=", wearableIds).Remove(0, 1);

        private async UniTaskVoid CheckForSendingPendingRequestsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.DelayFrame(FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS, cancellationToken: ct);

                if (pendingWearablesToRequest.Count <= 0)
                    continue;

                string[] wearablesToRequest = pendingWearablesToRequest.ToArray();
                pendingWearablesToRequest.Clear();

                var requestedWearables = await RequestWearablesAsync(wearablesToRequest, ct);
                List<string> wearablesNotFound = wearablesToRequest.ToList();
                foreach (WearableItem wearable in requestedWearables)
                {
                    wearablesNotFound.Remove(wearable.id);
                    ResolvePendingWearableById(wearable.id, wearable);
                }

                foreach (string wearableNotFound in wearablesNotFound)
                    ResolvePendingWearableById(wearableNotFound, null);
            }
        }

        private async UniTaskVoid CheckForUnusedWearablesAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(TIME_TO_CHECK_FOR_UNUSED_WEARABLES), cancellationToken: ct);

                if (wearablesInUseCounters.Count <= 0)
                    continue;

                var wearablesToRemove = from wearableInUse in wearablesInUseCounters
                    where wearableInUse.Value <= 0
                    select wearableInUse.Key;

                RemoveWearablesFromCatalog(wearablesToRemove);
            }
        }

        private void ResolvePendingWearableById(string wearableId, WearableItem result, string errorMessage = "")
        {
            if (!awaitingWearableTasks.TryGetValue(wearableId, out var task))
                return;

            if (string.IsNullOrEmpty(errorMessage))
                task.TrySetResult(result);
            else
                task.TrySetException(new Exception(errorMessage));

            awaitingWearableTasks.Remove(wearableId);
        }
    }
}
