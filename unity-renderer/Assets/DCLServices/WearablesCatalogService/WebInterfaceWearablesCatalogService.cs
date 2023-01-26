﻿using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCLServices.WearablesCatalogService
{
    [Obsolete("This service will be deprecated by LambdasWearablesCatalogService in the future.")]
    public class WebInterfaceWearablesCatalogService : MonoBehaviour, IWearablesCatalogService
    {
        private const string OWNED_WEARABLES_CONTEXT = "OwnedWearables";
        private const string BASE_WEARABLES_CONTEXT = "BaseWearables";
        private const string THIRD_PARTY_WEARABLES_CONTEXT = "ThirdPartyWearables";
        private const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";
        private const float REQUESTS_TIME_OUT_SECONDS = 45;
        private const float TIME_TO_CHECK_FOR_UNUSED_WEARABLES = 10f;
        private const int FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS = 1;

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

            // All the requests happened during the same frames interval are sent together
            CheckForSendingPendingRequestsAsync(serviceCts.Token).Forget();
            CheckForRequestsTimeOutsAsync(serviceCts.Token).Forget();
            CheckForRequestsByContextTimeOutsAsync(serviceCts.Token).Forget();

            // Check unused wearables (to be removed from our catalog) only every [TIME_TO_CHECK_FOR_UNUSED_WEARABLES] seconds
            CheckForUnusedWearablesAsync(serviceCts.Token).Forget();
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

        public async UniTask<WearableItem[]> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct) =>
            await RequestWearablesByContextAsync(userId, null, null, $"{OWNED_WEARABLES_CONTEXT}{userId}", false, ct);

        public async UniTask<WearableItem[]> RequestBaseWearablesAsync(CancellationToken ct) =>
            await RequestWearablesByContextAsync(null, null, new [] { BASE_WEARABLES_COLLECTION_ID }, BASE_WEARABLES_CONTEXT, false, ct);

        public async UniTask<WearableItem[]> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, CancellationToken ct) =>
            await RequestWearablesByContextAsync(null, null, null, $"{THIRD_PARTY_WEARABLES_CONTEXT}_{collectionId}", true, ct);

        public async UniTask<WearableItem[]> RequestWearablesAsync(string[] wearableIds, CancellationToken ct) =>
            await RequestWearablesByContextAsync(null, wearableIds, null, string.Join(",", wearableIds.ToArray()), false, ct);

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
                UniTaskCompletionSource<WearableItem> taskResult;

                if (!awaitingWearableTasks.ContainsKey(wearableId))
                {
                    taskResult = new UniTaskCompletionSource<WearableItem>();
                    awaitingWearableTasks.Add(wearableId, taskResult);

                    // We accumulate all the requests during the same frames interval to send them all together
                    pendingWearablesToRequest.Add(wearableId);
                }
                else
                    awaitingWearableTasks.TryGetValue(wearableId, out taskResult);

                return taskResult != null ? await taskResult.Task : null;
            }
            catch (Exception e) when (e is not OperationCanceledException) { throw; }
        }

        private async UniTask<WearableItem[]> RequestWearablesByContextAsync(
            string userId,
            string[] wearableIds,
            string[] collectionIds,
            string context,
            bool isThirdParty,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
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
                    awaitingWearablesByContextTasks.TryGetValue(context, out taskResult);

                WearableItem[] wearablesResult = Array.Empty<WearableItem>();

                if (taskResult != null)
                {
                    wearablesResult = await taskResult.Task;
                    AddWearablesToCatalog(wearablesResult);
                }

                return wearablesResult;
            }
            catch (Exception e) when (e is not OperationCanceledException) { throw; }
        }

        [UsedImplicitly]
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

            AddWearablesToCatalog(request.wearables);

            if (!string.IsNullOrEmpty(request.context))
            {
                ResolvePendingWearablesByContext(request.context, request.wearables);
                pendingWearablesByContextRequestedTimes.Remove(request.context);
            }
        }

        [UsedImplicitly]
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

        private async UniTaskVoid CheckForSendingPendingRequestsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.DelayFrame(FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS, cancellationToken: ct);

                if (pendingWearablesToRequest.Count <= 0)
                    continue;

                string[] wearablesToRequest = pendingWearablesToRequest.ToArray();
                pendingWearablesToRequest.Clear();

                foreach (string wearablesToRequestId in wearablesToRequest)
                {
                    if (!pendingWearableRequestedTimes.ContainsKey(wearablesToRequestId))
                        pendingWearableRequestedTimes.Add(wearablesToRequestId, Time.realtimeSinceStartup);
                }

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

        private async UniTaskVoid CheckForRequestsTimeOutsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.DelayFrame(FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS, cancellationToken: ct);

                if (pendingWearableRequestedTimes.Count <= 0)
                    continue;

                List<string> expiredRequests = (from taskRequestedTime in pendingWearableRequestedTimes
                    where Time.realtimeSinceStartup - taskRequestedTime.Value > REQUESTS_TIME_OUT_SECONDS
                    select taskRequestedTime.Key).ToList();

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
                await UniTask.DelayFrame(FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS, cancellationToken: ct);

                if (pendingWearablesByContextRequestedTimes.Count <= 0)
                    continue;

                List<string> expiredRequests = (from promiseByContextRequestedTime in pendingWearablesByContextRequestedTimes
                    where Time.realtimeSinceStartup - promiseByContextRequestedTime.Value > REQUESTS_TIME_OUT_SECONDS
                    select promiseByContextRequestedTime.Key).ToList();

                foreach (string expiredRequestToRemove in expiredRequests)
                {
                    pendingWearablesByContextRequestedTimes.Remove(expiredRequestToRemove);

                    ResolvePendingWearablesByContext(expiredRequestToRemove, null,
                        $"The request for the wearable context '{expiredRequestToRemove}' has exceed the set timeout!");
                }
            }
        }

        private async UniTaskVoid CheckForUnusedWearablesAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(TIME_TO_CHECK_FOR_UNUSED_WEARABLES), cancellationToken: ct);

                if (wearablesInUseCounters.Count <= 0)
                    continue;

                List<string> wearablesToRemove = (from wearableInUse in wearablesInUseCounters
                    where wearableInUse.Value <= 0
                    select wearableInUse.Key).ToList();

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
