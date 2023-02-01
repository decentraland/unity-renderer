﻿using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MainScripts.DCL.Helpers.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCLServices.WearablesCatalogService
{
    /// <summary>
    /// This service keeps the same logic of the old CatalogController (but managed with UniTasks instead of Promises)
    /// so all the wearables requests will pass through kernel.
    /// It will be deprecated once we move all the kernel's logic related to requesting wearables to Unity.
    /// </summary>
    [Obsolete("This service will be deprecated by LambdasWearablesCatalogService in the future.")]
    public class WebInterfaceWearablesCatalogService : MonoBehaviour, IWearablesCatalogService
    {
        private const string OWNED_WEARABLES_CONTEXT = "OwnedWearables";
        private const string BASE_WEARABLES_CONTEXT = "BaseWearables";
        private const string THIRD_PARTY_WEARABLES_CONTEXT = "ThirdPartyWearables";
        private const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";
        private const float REQUESTS_TIME_OUT_SECONDS = 45;

        public static WebInterfaceWearablesCatalogService Instance { get; private set; }
        public BaseDictionary<string, WearableItem> WearablesCatalog { get; private set; }

        private WearablesWebInterfaceBridge webInterfaceBridge;
        private CancellationTokenSource serviceCts;
        private readonly Dictionary<string, int> wearablesInUseCounters = new ();
        private readonly Dictionary<string, UniTaskCompletionSource<WearableItem[]>> awaitingWearablesByContextTasks = new ();
        private readonly Dictionary<string, float> pendingWearablesByContextRequestedTimes = new ();
        private readonly Dictionary<string, UniTaskCompletionSource<WearableItem>> awaitingWearableTasks = new ();
        private readonly Dictionary<string, float> pendingWearableRequestedTimes = new ();
        private readonly List<string> pendingWearablesToRequest = new ();

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            serviceCts = new CancellationTokenSource();

            try
            {
                CheckForRequestsTimeOutsAsync(serviceCts.Token).Forget();
                CheckForRequestsByContextTimeOutsAsync(serviceCts.Token).Forget();
            }
            catch (OperationCanceledException) { }
        }

        public void Initialize(
            WearablesWebInterfaceBridge wearablesWebInterfaceBridge,
            BaseDictionary<string, WearableItem> wearablesCatalog)
        {
            webInterfaceBridge = wearablesWebInterfaceBridge;
            WearablesCatalog = wearablesCatalog;
            Initialize();
        }

        public void Dispose()
        {
            Destroy(this);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct) =>
            await RequestWearablesByContextAsync(userId, null, null, $"{OWNED_WEARABLES_CONTEXT}{userId}", false, ct);

        public async UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct) =>
            await RequestWearablesByContextAsync(null, null, new [] { BASE_WEARABLES_COLLECTION_ID }, BASE_WEARABLES_CONTEXT, false, ct);

        public async UniTask<IReadOnlyList<WearableItem>> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct) =>
            await RequestWearablesByContextAsync(null, null, null, $"{THIRD_PARTY_WEARABLES_CONTEXT}_{collectionId}", true, ct);

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

        private async UniTask<WearableItem> SyncWearablesRequestsAsync(string newWearableId, CancellationToken ct)
        {
            if (!awaitingWearableTasks.TryGetValue(newWearableId, out var source))
            {
                pendingWearablesToRequest.Add(newWearableId);
                awaitingWearableTasks[newWearableId] = source = new UniTaskCompletionSource<WearableItem>();
            }

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: ct);

            WearableItem result = null;

            if (pendingWearablesToRequest.Count > 0)
            {
                using var wearableIdsPool = PoolUtils.RentList<string>();
                var wearableIds = wearableIdsPool.GetList();
                wearableIds.AddRange(pendingWearablesToRequest);
                pendingWearablesToRequest.Clear();

                foreach (string id in wearableIds)
                {
                    if (!pendingWearableRequestedTimes.ContainsKey(id))
                        pendingWearableRequestedTimes.Add(id, Time.realtimeSinceStartup);
                }

                IReadOnlyList<WearableItem> wearablesRequested;

                try
                {
                    wearablesRequested = await RequestWearablesByContextAsync(null, wearableIds, null, string.Join(",", wearableIds), false, serviceCts.Token);
                }
                catch (Exception e)
                {
                    source.TrySetException(e);
                    throw;
                }

                AddWearablesToCatalog(wearablesRequested);

                // Resolves found wearables
                foreach (WearableItem wearable in wearablesRequested)
                {
                    ResolvePendingWearableById(wearable.id, wearable);

                    // if (!awaitingWearableTasks.TryGetValue(wearable.id, out var wearableSource))
                    //     continue;
                    //
                    // wearableSource.TrySetResult(wearable);
                    // awaitingWearableTasks.Remove(wearable.id);

                    if (wearable.id == newWearableId)
                        result = wearable;
                }

                // Resolves not found wearables
                foreach (string id in wearableIds)
                {
                    ResolvePendingWearableById(id, null);

                    // if (!awaitingWearableTasks.TryGetValue(id, out var wearableNotFoundSource))
                    //     continue;
                    //
                    // wearableNotFoundSource.TrySetResult(null);
                    // awaitingWearableTasks.Remove(id);
                }
            }
            else
                result = await source.Task;

            ct.ThrowIfCancellationRequested();

            return result;
        }

        private async UniTask<IReadOnlyList<WearableItem>> RequestWearablesByContextAsync(
            string userId,
            List<string> wearableIds,
            string[] collectionIds,
            string context,
            bool isThirdParty,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            UniTaskCompletionSource<WearableItem[]> taskResult;

            if (!awaitingWearablesByContextTasks.ContainsKey(context))
            {
                taskResult = new UniTaskCompletionSource<WearableItem[]>();
                awaitingWearablesByContextTasks.Add(context, taskResult);

                if (!pendingWearablesByContextRequestedTimes.ContainsKey(context))
                    pendingWearablesByContextRequestedTimes.Add(context, Time.realtimeSinceStartup);

                if (!isThirdParty)
                    webInterfaceBridge.RequestWearables(userId, wearableIds, collectionIds, context);
                else
                    webInterfaceBridge.RequestThirdPartyWearables(userId, collectionIds[0], context);
            }
            else
                taskResult = awaitingWearablesByContextTasks[context];

            ct.RegisterWithoutCaptureExecutionContext(() => taskResult.TrySetCanceled());
            var wearablesResult = await taskResult.Task;
            AddWearablesToCatalog(wearablesResult);

            return wearablesResult;
        }

        [PublicAPI]
        public void AddWearablesToCatalog(string payload)
        {
            WearablesRequestResponse request = null;

            try
            {
                // The new wearables paradigm is based on composing with optional field
                // i.e. the emotes will have an emoteDataV0 property with some values.
                // JsonUtility.FromJson doesn't allow null properties so we have to use Newtonsoft instead
                request = JsonConvert.DeserializeObject<WearablesRequestResponse>(payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"Fail to parse wearables json {e}");
            }

            if (request == null)
                return;

            if (!string.IsNullOrEmpty(request.context))
                ResolvePendingWearablesByContext(request.context, request.wearables);
        }

        [PublicAPI]
        public void WearablesRequestFailed(string payload)
        {
            WearablesRequestFailed requestFailedResponse = JsonUtility.FromJson<WearablesRequestFailed>(payload);

            if (requestFailedResponse.context == BASE_WEARABLES_CONTEXT ||
                requestFailedResponse.context.Contains(THIRD_PARTY_WEARABLES_CONTEXT) ||
                requestFailedResponse.context.Contains(OWNED_WEARABLES_CONTEXT))
            {
                ResolvePendingWearablesByContext(requestFailedResponse.context, null, requestFailedResponse.error);
            }
            else
            {
                string[] failedWearablesIds = requestFailedResponse.context.Split(',');
                foreach (string failedWearableId in failedWearablesIds)
                {
                    ResolvePendingWearableById(
                        failedWearableId,
                        null,
                        $"The request for the wearable '{failedWearableId}' has failed: {requestFailedResponse.error}");
                }
            }
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
                    wearablesInUseCounters[wearableItem.id] = 10000; //A high value to ensure they are not removed
            }
        }

        public void Clear()
        {
            WearablesCatalog.Clear();
            wearablesInUseCounters.Clear();
            awaitingWearableTasks.Clear();
            awaitingWearablesByContextTasks.Clear();
            pendingWearablesToRequest.Clear();
            pendingWearableRequestedTimes.Clear();
            pendingWearablesByContextRequestedTimes.Clear();
        }

        private async UniTaskVoid CheckForRequestsTimeOutsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: ct);

                if (pendingWearableRequestedTimes.Count <= 0)
                    continue;

                var expiredRequests = from taskRequestedTime in pendingWearableRequestedTimes
                    where Time.realtimeSinceStartup - taskRequestedTime.Value > REQUESTS_TIME_OUT_SECONDS
                    select taskRequestedTime.Key;

                foreach (string expiredRequestId in expiredRequests)
                {
                    pendingWearableRequestedTimes.Remove(expiredRequestId);
                    ResolvePendingWearableById(expiredRequestId, null,
                        $"The request for the wearable '{expiredRequestId}' has exceed the set timeout!");
                }
            }
        }

        private async UniTaskVoid CheckForRequestsByContextTimeOutsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: ct);

                if (pendingWearablesByContextRequestedTimes.Count <= 0)
                    continue;

                var expiredRequests = from promiseByContextRequestedTime in pendingWearablesByContextRequestedTimes
                    where Time.realtimeSinceStartup - promiseByContextRequestedTime.Value > REQUESTS_TIME_OUT_SECONDS
                    select promiseByContextRequestedTime.Key;

                foreach (string expiredRequestToRemove in expiredRequests)
                {
                    pendingWearablesByContextRequestedTimes.Remove(expiredRequestToRemove);

                    ResolvePendingWearablesByContext(expiredRequestToRemove, null,
                        $"The request for the wearable context '{expiredRequestToRemove}' has exceed the set timeout!");
                }
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
            pendingWearableRequestedTimes.Remove(wearableId);
        }

        private void ResolvePendingWearablesByContext(string context, WearableItem[] newWearablesAddedIntoCatalog = null, string errorMessage = "")
        {
            if (!awaitingWearablesByContextTasks.TryGetValue(context, out var task))
                return;

            if (string.IsNullOrEmpty(errorMessage))
                task.TrySetResult(newWearablesAddedIntoCatalog);
            else
                task.TrySetException(new Exception(errorMessage));

            awaitingWearablesByContextTasks.Remove(context);
            pendingWearablesByContextRequestedTimes.Remove(context);
        }
    }
}
