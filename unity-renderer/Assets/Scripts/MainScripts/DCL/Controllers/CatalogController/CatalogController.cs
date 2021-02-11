using DCL;
using DCL.Helpers;
using DCL.Interface;
using System.Collections.Generic;
using UnityEngine;
using static DCL.Interface.WebInterface;

public class CatalogController : MonoBehaviour
{
    public static bool VERBOSE = false;
    private const string OWNED_WEARABLES_CONTEXT = "OwnedWearables";
    private const string BASE_WEARABLES_CONTEXT = "BaseWearables";
    private const int FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS = 1;
    private const float TIME_TO_CHECK_FOR_UNUSED_WEARABLES = 10f;
    private const float REQUESTS_TIME_OUT = 5f;

    public static CatalogController i { get; private set; }

    public static BaseDictionary<string, Item> itemCatalog => DataStore.i.items;
    public static BaseDictionary<string, WearableItem> wearableCatalog => DataStore.i.wearables;

    private static Dictionary<string, int> wearablesInUseCounters = new Dictionary<string, int>();
    private static Dictionary<string, Promise<WearableItem>> awaitingWearablePromises = new Dictionary<string, Promise<WearableItem>>();
    private static Dictionary<string, float> pendingWearableRequestedTimes = new Dictionary<string, float>();
    private static Dictionary<string, Promise<WearableItem[]>> awaitingWearablesByContextPromises = new Dictionary<string, Promise<WearableItem[]>>();
    private static Dictionary<string, float> pendingWearablesByContextRequestedTimes = new Dictionary<string, float>();
    private static List<string> pendingRequestsToSend = new List<string>();
    private float timeSinceLastUnusedWearablesCheck = 0f;

    

    public void Awake()
    {
        i = this;
    }

    private void Update()
    {
        // All the requests happened during the same frames interval are sent together
        if (Time.frameCount % FRAMES_TO_CHECK_FOR_SENDING_PENDING_REQUESTS == 0)
        {
            CheckForSendingPendingRequests();
            CheckForRequestsTimeOuts();
            CheckForRequestsByContextTimeOuts();
        }

        // Check unused wearables (to be removed from our catalog) only every [TIME_TO_CHECK_FOR_UNUSED_WEARABLES] seconds
        timeSinceLastUnusedWearablesCheck += Time.deltaTime;
        if (timeSinceLastUnusedWearablesCheck >= TIME_TO_CHECK_FOR_UNUSED_WEARABLES)
        {
            CheckForUnusedWearables();
            timeSinceLastUnusedWearablesCheck = 0f;
        }
    }

    public void AddWearablesToCatalog(string payload)
    {
        WearablesRequestResponse request = JsonUtility.FromJson<WearablesRequestResponse>(payload);

        if (VERBOSE)
            Debug.Log("add wearables: " + payload);

        for (int i = 0; i < request.wearables.Length; i++)
        {
            switch (request.wearables[i].type)
            {
                case "wearable":
                    {
                        WearableItem wearableItem = request.wearables[i];

                        if (!wearableCatalog.ContainsKey(wearableItem.id))
                        {
                            wearableCatalog.Add(wearableItem.id, wearableItem);
                            wearablesInUseCounters.Add(wearableItem.id, 1);
                            ResolvePendingWearablePromise(wearableItem.id, wearableItem);
                            pendingWearableRequestedTimes.Remove(wearableItem.id);
                        }

                        break;
                    }
                case "item":
                    {
                        if (!itemCatalog.ContainsKey(request.wearables[i].id))
                            itemCatalog.Add(request.wearables[i].id, (Item)request.wearables[i]);

                        break;
                    }
                default:
                    {
                        Debug.LogError("Bad type in item, will not be added to catalog");
                        break;
                    }
            }
        }

        if (!string.IsNullOrEmpty(request.context))
        {
            ResolvePendingWearablesByContextPromise(request.context, request.wearables);
            pendingWearablesByContextRequestedTimes.Remove(request.context);
        }
    }

    public void WearablesRequestFailed(string payload)
    {
        WearablesRequestFailed requestFailedResponse = JsonUtility.FromJson<WearablesRequestFailed>(payload);

        if (!string.IsNullOrEmpty(requestFailedResponse.context))
        {
            ResolvePendingWearablesByContextPromise(
                requestFailedResponse.context,
                null,
                requestFailedResponse.error);
        }
        else
        {
            Debug.LogError(requestFailedResponse.error);
        }
    }

    public void RemoveWearablesFromCatalog(string payload)
    {
        string[] itemIDs = JsonUtility.FromJson<string[]>(payload);

        int count = itemIDs.Length;
        for (int i = 0; i < count; ++i)
        {
            itemCatalog.Remove(itemIDs[i]);
            wearableCatalog.Remove(itemIDs[i]);
            wearablesInUseCounters.Remove(itemIDs[i]);
        }
    }

    public void ClearWearableCatalog()
    {
        itemCatalog?.Clear();
        wearableCatalog?.Clear();
        wearablesInUseCounters.Clear();
    }

    public static Promise<WearableItem> RequestWearable(string wearableId)
    {
        Promise<WearableItem> promiseResult;

        if (wearableCatalog.TryGetValue(wearableId, out WearableItem wearable))
        {
            wearablesInUseCounters[wearableId]++;
            promiseResult = new Promise<WearableItem>();
            promiseResult.Resolve(wearable);
        }
        else
        {
            if (!awaitingWearablePromises.ContainsKey(wearableId))
            {
                promiseResult = new Promise<WearableItem>();
                awaitingWearablePromises.Add(wearableId, promiseResult);

                // We accumulate all the requests during the same frames interval to send them all together
                pendingRequestsToSend.Add(wearableId);
            }
            else
            {
                awaitingWearablePromises.TryGetValue(wearableId, out promiseResult);
            }
        }

        return promiseResult;
    }

    public static Promise<WearableItem[]> RequestOwnedWearables(string userId)
    {
        Promise<WearableItem[]> promiseResult;

        if (!awaitingWearablesByContextPromises.ContainsKey(OWNED_WEARABLES_CONTEXT))
        {
            promiseResult = new Promise<WearableItem[]>();
            awaitingWearablesByContextPromises.Add(OWNED_WEARABLES_CONTEXT, promiseResult);

            if (!pendingWearablesByContextRequestedTimes.ContainsKey(OWNED_WEARABLES_CONTEXT))
                pendingWearablesByContextRequestedTimes.Add(OWNED_WEARABLES_CONTEXT, Time.realtimeSinceStartup);

            WebInterface.RequestWearables(
                ownedByUser: userId,
                wearableIds: null,
                collectionIds: null,
                context: OWNED_WEARABLES_CONTEXT
            );
        }
        else
        {
            awaitingWearablesByContextPromises.TryGetValue(OWNED_WEARABLES_CONTEXT, out promiseResult);
        }

        return promiseResult;
    }

    public static Promise<WearableItem[]> RequestBaseWearables()
    {
        Promise<WearableItem[]> promiseResult;

        if (!awaitingWearablesByContextPromises.ContainsKey(BASE_WEARABLES_CONTEXT))
        {
            promiseResult = new Promise<WearableItem[]>();
            awaitingWearablesByContextPromises.Add(BASE_WEARABLES_CONTEXT, promiseResult);

            if (!pendingWearablesByContextRequestedTimes.ContainsKey(BASE_WEARABLES_CONTEXT))
                pendingWearablesByContextRequestedTimes.Add(BASE_WEARABLES_CONTEXT, Time.realtimeSinceStartup);

            WebInterface.RequestWearables(
                ownedByUser: null,
                wearableIds: null,
                collectionIds: new string[] { "base-avatars" },
                context: BASE_WEARABLES_CONTEXT
            );
        }
        else
        {
            awaitingWearablesByContextPromises.TryGetValue(BASE_WEARABLES_CONTEXT, out promiseResult);
        }

        return promiseResult;
    }

    public static void RemoveWearablesInUse(List<string> wearablesInUseToRemove)
    {
        foreach (var wearableToRemove in wearablesInUseToRemove)
        {
            if (wearablesInUseCounters.ContainsKey(wearableToRemove))
            {
                wearablesInUseCounters[wearableToRemove]--;
            }
        }
    }

    private void ResolvePendingWearablePromise(string wearableId, WearableItem newWearableAddedIntoCatalog = null, string errorMessage = "")
    {
        if (awaitingWearablePromises.TryGetValue(wearableId, out Promise<WearableItem> promise))
        {
            if (string.IsNullOrEmpty(errorMessage))
                promise.Resolve(newWearableAddedIntoCatalog);
            else
                promise.Reject(errorMessage);

            awaitingWearablePromises.Remove(wearableId);
        }
    }

    private void ResolvePendingWearablesByContextPromise(string context, WearableItem[] newWearablesAddedIntoCatalog = null, string errorMessage = "")
    {
        if (awaitingWearablesByContextPromises.TryGetValue(context, out Promise<WearableItem[]> promise))
        {
            if (string.IsNullOrEmpty(errorMessage))
                promise.Resolve(newWearablesAddedIntoCatalog);
            else
                promise.Reject(errorMessage);

            awaitingWearablesByContextPromises.Remove(context);
        }
    }

    private void CheckForSendingPendingRequests()
    {
        if (pendingRequestsToSend.Count > 0)
        {
            foreach (var request in pendingRequestsToSend)
            {
                if (!pendingWearableRequestedTimes.ContainsKey(request))
                    pendingWearableRequestedTimes.Add(request, Time.realtimeSinceStartup);
            }

            WebInterface.RequestWearables(
                ownedByUser: null,
                wearableIds: pendingRequestsToSend.ToArray(),
                collectionIds: null,
                context: null
            );

            pendingRequestsToSend.Clear();
        }
    }

    private void CheckForRequestsTimeOuts()
    {
        if (pendingWearableRequestedTimes.Count > 0)
        {
            List<string> expiredRequestedTimes = new List<string>();
            foreach (var promiseRequestedTime in pendingWearableRequestedTimes)
            {
                if ((Time.realtimeSinceStartup - promiseRequestedTime.Value) > REQUESTS_TIME_OUT)
                {
                    ResolvePendingWearablePromise(
                        promiseRequestedTime.Key,
                        null,
                        $"The request for the wearable '{promiseRequestedTime.Key}' has exceed the set timeout!");
                    expiredRequestedTimes.Add(promiseRequestedTime.Key);
                }
            }

            foreach (var expiredTimeToRemove in expiredRequestedTimes)
            {
                pendingWearableRequestedTimes.Remove(expiredTimeToRemove);
            }
        }
    }

    private void CheckForRequestsByContextTimeOuts()
    {
        if (pendingWearablesByContextRequestedTimes.Count > 0)
        {
            List<string> expiredRequestedTimes = new List<string>();
            foreach (var promiseByContextRequestedTime in pendingWearablesByContextRequestedTimes)
            {
                if ((Time.realtimeSinceStartup - promiseByContextRequestedTime.Value) > REQUESTS_TIME_OUT)
                {
                    ResolvePendingWearablesByContextPromise(
                        promiseByContextRequestedTime.Key,
                        null,
                        $"The request for the wearable context '{promiseByContextRequestedTime.Key}' has exceed the set timeout!");
                    expiredRequestedTimes.Add(promiseByContextRequestedTime.Key);
                }
            }

            foreach (var expiredTimeToRemove in expiredRequestedTimes)
            {
                pendingWearablesByContextRequestedTimes.Remove(expiredTimeToRemove);
            }
        }
    }

    private void CheckForUnusedWearables()
    {
        if (wearablesInUseCounters.Count > 0)
        {
            List<string> wearablesToDestroy = new List<string>();
            foreach (var wearableInUse in wearablesInUseCounters)
            {
                if (wearableInUse.Value <= 0)
                {
                    wearablesToDestroy.Add(wearableInUse.Key);
                }
            }

            foreach (var wearableToDestroy in wearablesToDestroy)
            {
                wearableCatalog.Remove(wearableToDestroy);
                wearablesInUseCounters.Remove(wearableToDestroy);
            }
        }
    }
}
