using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLServices.Lambdas;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.WearablesCatalogService
{
    /// <summary>
    /// This service implements a direct way of getting wearables sending the requests directly to lambdas.
    /// </summary>
    public class LambdasWearablesCatalogService : IWearablesCatalogService, ILambdaServiceConsumer<WearableWithDefinitionResponse>
    {
        public BaseDictionary<string, WearableItem> WearablesCatalog { get; }

        private const string ASSET_BUNDLES_URL_ORG = "https://content-assets-as-bundle.decentraland.org/";
        private const string PAGINATED_WEARABLES_END_POINT = "users/";
        private const string NON_PAGINATED_WEARABLES_END_POINT = "collections/wearables/";
        private const string BASE_WEARABLES_COLLECTION_ID = "urn:decentraland:off-chain:base-avatars";
        private const string THIRD_PARTY_COLLECTIONS_FETCH_URL = "third-party-integrations";
        private const int REQUESTS_TIME_OUT_SECONDS = 45;
        private const int MAX_WEARABLES_PER_REQUEST = 200;

        private readonly ILambdasService lambdasService;
        private readonly IServiceProviders serviceProviders;
        private readonly Dictionary<string, int> wearablesInUseCounters = new ();
        private readonly Dictionary<(string userId, int pageSize), LambdaResponsePagePointer<WearableWithDefinitionResponse>> ownerWearablesPagePointers = new ();
        private readonly Dictionary<(string userId, string collectionId, int pageSize), LambdaResponsePagePointer<WearableWithDefinitionResponse>> thirdPartyCollectionPagePointers = new ();
        private readonly List<string> pendingWearablesToRequest = new ();
        private CancellationTokenSource serviceCts;
        private UniTaskCompletionSource<IReadOnlyList<WearableItem>> lastRequestSource;
        private ICatalyst catalyst;

#if UNITY_EDITOR
        private readonly DebugConfig debugConfig = DataStore.i.debugConfig;
#endif
        public LambdasWearablesCatalogService(BaseDictionary<string, WearableItem> wearablesCatalog,
            ILambdasService lambdasService,
            IServiceProviders serviceProviders)
        {
            this.lambdasService = lambdasService;
            this.serviceProviders = serviceProviders;
            WearablesCatalog = wearablesCatalog;
            catalyst = serviceProviders.catalyst;
        }

        public void Initialize() =>
            serviceCts = serviceCts.SafeRestart();

        public void Dispose()
        {
            serviceCts.SafeCancelAndDispose();
            serviceCts = null;
            Clear();
        }

        public async UniTask<WearableCollectionsAPIData.Collection[]> GetThirdPartyCollectionsAsync(CancellationToken cancellationToken)
        {
            (WearableCollectionsAPIData response, bool success) = await lambdasService.Get<WearableCollectionsAPIData>(THIRD_PARTY_COLLECTIONS_FETCH_URL,
                THIRD_PARTY_COLLECTIONS_FETCH_URL, cancellationToken: cancellationToken);

            if (!success)
                throw new Exception("Request error! third party collections couldn't be fetched!");

            return response.data;
        }

        public async UniTask<(IReadOnlyList<WearableItem> wearables, int totalAmount)> RequestOwnedWearablesAsync(
            string userId, int pageNumber, int pageSize, CancellationToken cancellationToken, string category = null,
            NftRarity rarity = NftRarity.None,
            NftCollectionType collectionTypeMask = NftCollectionType.All,
            ICollection<string> thirdPartyCollectionIds = null,
            string name = null, (NftOrderByOperation type, bool directionAscendent)? orderBy = null)
        {
            var queryParams = new List<(string name, string value)>
            {
                ("pageNum", pageNumber.ToString()),
                ("pageSize", pageSize.ToString()),
                ("includeEntities", "true"),
            };

            if (rarity != NftRarity.None)
                queryParams.Add(("rarity", rarity.ToString().ToLower()));

            if (!string.IsNullOrEmpty(category))
                queryParams.Add(("category", category));

            if (!string.IsNullOrEmpty(name))
                queryParams.Add(("name", name));

            if (orderBy != null)
            {
                queryParams.Add(("orderBy", orderBy.Value.type.ToString().ToLower()));
                queryParams.Add(("direction", orderBy.Value.directionAscendent ? "ASC" : "DESC"));
            }

            if ((collectionTypeMask & NftCollectionType.Base) != 0)
                queryParams.Add(("collectionType", "base-wearable"));

            if ((collectionTypeMask & NftCollectionType.OnChain) != 0)
                queryParams.Add(("collectionType", "on-chain"));

            if ((collectionTypeMask & NftCollectionType.ThirdParty) != 0)
                queryParams.Add(("collectionType", "third-party"));

            if (thirdPartyCollectionIds != null)
                foreach (string collectionId in thirdPartyCollectionIds)
                    queryParams.Add(("thirdPartyCollectionId", collectionId));

            await UniTask.WaitUntil(() => catalyst.lambdasUrl != null, cancellationToken: cancellationToken);
            string explorerUrl = catalyst.lambdasUrl.Replace("/lambdas", "/explorer");

            (WearableWithEntityResponseDto response, bool success) = await lambdasService.GetFromSpecificUrl<WearableWithEntityResponseDto>(
                $"{explorerUrl}/:userId/wearables",
                $"{explorerUrl}/{userId}/wearables",
                cancellationToken: cancellationToken,
                urlEncodedParams: queryParams.ToArray());

            if (!success)
                throw new Exception($"The request of wearables for '{userId}' failed!");

            List<WearableItem> wearables = ValidateWearables(response.elements,
                $"{catalyst.contentUrl}/contents/",
                ASSET_BUNDLES_URL_ORG);

            AddWearablesToCatalog(wearables);

            return (wearables, response.TotalAmount);
        }

        public async UniTask<(IReadOnlyList<WearableItem> wearables, int totalAmount)> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {

#if UNITY_EDITOR
            string debugUserId = debugConfig.overrideUserID;

            if (!string.IsNullOrEmpty(debugUserId))
                userId = debugUserId;
#endif

            var createNewPointer = false;

            if (!ownerWearablesPagePointers.TryGetValue((userId, pageSize), out var pagePointer)) { createNewPointer = true; }
            else if (cleanCachedPages)
            {
                pagePointer.Dispose();
                ownerWearablesPagePointers.Remove((userId, pageSize));
                createNewPointer = true;
            }

            if (createNewPointer)
            {
                ownerWearablesPagePointers[(userId, pageSize)] = pagePointer = new LambdaResponsePagePointer<WearableWithDefinitionResponse>(
                    $"{PAGINATED_WEARABLES_END_POINT}{userId}/wearables",
                    pageSize, ct, this);
            }

            var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);

            if (!pageResponse.success)
                throw new Exception($"The request of the owned wearables for '{userId}' failed!");

            var wearables = pageResponse.response.elements.Select(x => x.definition).ToList();
            MapLambdasDataIntoWearableItem(wearables);
            AddWearablesToCatalog(wearables);

            return (wearables, pageResponse.response.TotalAmount);
        }

        public async UniTask<IReadOnlyList<WearableItem>> RequestBaseWearablesAsync(CancellationToken ct)
        {
            var serviceResponse = await lambdasService.Get<WearableWithoutDefinitionResponse>(
                NON_PAGINATED_WEARABLES_END_POINT,
                NON_PAGINATED_WEARABLES_END_POINT,
                REQUESTS_TIME_OUT_SECONDS,
                urlEncodedParams: ("collectionId", BASE_WEARABLES_COLLECTION_ID),
                cancellationToken: ct);

            if (!serviceResponse.success)
                throw new Exception("The request of the base wearables failed!");

            MapLambdasDataIntoWearableItem(serviceResponse.response.wearables);
            AddWearablesToCatalog(serviceResponse.response.wearables);

            return serviceResponse.response.wearables;
        }

        public async UniTask<(IReadOnlyList<WearableItem> wearables, int totalAmount)> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, int pageNumber, int pageSize, bool cleanCachedPages,
            CancellationToken ct)
        {
            var createNewPointer = false;

            if (!thirdPartyCollectionPagePointers.TryGetValue((userId, collectionId, pageSize), out var pagePointer)) { createNewPointer = true; }
            else if (cleanCachedPages)
            {
                pagePointer.Dispose();
                thirdPartyCollectionPagePointers.Remove((userId, collectionId, pageSize));
                createNewPointer = true;
            }

            if (createNewPointer)
            {
                thirdPartyCollectionPagePointers[(userId, collectionId, pageSize)] = pagePointer = new LambdaResponsePagePointer<WearableWithDefinitionResponse>(
                    $"{PAGINATED_WEARABLES_END_POINT}{userId}/third-party-wearables/{collectionId}",
                    pageSize, ct, this);
            }

            var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);

            if (!pageResponse.success)
                throw new Exception($"The request of the '{collectionId}' third party wearables collection of '{userId}' failed!");

            var wearables = pageResponse.response.elements.Select(x => x.definition).ToList();
            MapLambdasDataIntoWearableItem(wearables);
            AddWearablesToCatalog(wearables);

            return (wearables, pageResponse.response.TotalAmount);
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
                RemoveWearableFromCatalog(wearableId);
        }

        public void RemoveWearableFromCatalog(string wearableId)
        {
            WearablesCatalog.Remove(wearableId);
            wearablesInUseCounters.Remove(wearableId);
        }

        public void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove)
        {
            foreach (string wearableToRemove in wearablesInUseToRemove)
            {
                if (!wearablesInUseCounters.ContainsKey(wearableToRemove))
                    continue;

                wearablesInUseCounters[wearableToRemove]--;

                if (wearablesInUseCounters[wearableToRemove] > 0)
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
            ownerWearablesPagePointers.Clear();
            thirdPartyCollectionPagePointers.Clear();
        }

        public bool IsValidWearable(string wearableId)
        {
            if (!WearablesCatalog.TryGetValue(wearableId, out var wearable))
                return false;

            return wearable != null;
        }

        UniTask<(WearableWithDefinitionResponse response, bool success)> ILambdaServiceConsumer<WearableWithDefinitionResponse>.CreateRequest
            (string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            lambdasService.Get<WearableWithDefinitionResponse>(
                PAGINATED_WEARABLES_END_POINT,
                endPoint,
                REQUESTS_TIME_OUT_SECONDS,
                ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
                cancellationToken,
                LambdaPaginatedResponseHelper.GetPageSizeParam(pageSize),
                LambdaPaginatedResponseHelper.GetPageNumParam(pageNumber),
                ("includeDefinitions", "true"));

        private async UniTask<WearableItem> SyncWearablesRequestsAsync(string newWearableId, CancellationToken ct)
        {
            pendingWearablesToRequest.Add(newWearableId);
            lastRequestSource ??= new UniTaskCompletionSource<IReadOnlyList<WearableItem>>();
            var sourceToAwait = lastRequestSource;

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: ct);

            List<WearableItem> result = new List<WearableItem>();

            if (pendingWearablesToRequest.Count > 0)
            {
                lastRequestSource = null;

                using var wearableIdsPool = PoolUtils.RentList<string>();
                var wearableIds = wearableIdsPool.GetList();
                wearableIds.AddRange(pendingWearablesToRequest);
                pendingWearablesToRequest.Clear();

                // When the number of wearables to request is greater than MAX_WEARABLES_PER_REQUEST, we split the request into several smaller ones.
                // In this way we avoid to send a very long url string that would fail due to the web request size limitations.
                int numberOfPartialRequests = (wearableIds.Count + MAX_WEARABLES_PER_REQUEST - 1) / MAX_WEARABLES_PER_REQUEST;
                var awaitingPartialTasksPool = PoolUtils.RentList<(UniTask<(WearableWithoutDefinitionResponse response, bool success)> task, IEnumerable<string> wearablesRequested)>();
                var awaitingPartialTasks = awaitingPartialTasksPool.GetList();

                for (var i = 0; i < numberOfPartialRequests; i++)
                {
                    int numberOfWearablesToRequest = wearableIds.Count < MAX_WEARABLES_PER_REQUEST
                        ? wearableIds.Count
                        : MAX_WEARABLES_PER_REQUEST;

                    var wearablesToRequest = wearableIds.Take(numberOfWearablesToRequest).ToList();

                    var partialTask = lambdasService.Get<WearableWithoutDefinitionResponse>(
                        NON_PAGINATED_WEARABLES_END_POINT,
                        NON_PAGINATED_WEARABLES_END_POINT,
                        REQUESTS_TIME_OUT_SECONDS,
                        urlEncodedParams: GetWearablesUrlParams(wearablesToRequest),
                        cancellationToken: serviceCts.Token);

                    wearableIds.RemoveRange(0, numberOfWearablesToRequest);
                    awaitingPartialTasks.Add((partialTask, wearablesToRequest));
                }

                var servicePartialResponsesPool = PoolUtils.RentList<((WearableWithoutDefinitionResponse response, bool success) taskResponse, IEnumerable<string> wearablesRequested)>();
                var servicePartialResponses = servicePartialResponsesPool.GetList();

                try
                {
                    foreach (var partialTask in awaitingPartialTasks)
                        servicePartialResponses.Add((await partialTask.task, partialTask.wearablesRequested));
                }
                catch (Exception e)
                {
                    sourceToAwait.TrySetException(e);
                    throw;
                }

                foreach (var partialResponse in servicePartialResponses)
                {
                    if (!partialResponse.taskResponse.success)
                    {
                        Exception e = new Exception($"The request of the wearables ('{string.Join(", ", partialResponse.wearablesRequested)}') failed!");
                        sourceToAwait.TrySetException(e);
                        throw e;
                    }

                    var wearables = partialResponse.taskResponse.response.wearables;

                    MapLambdasDataIntoWearableItem(wearables);
                    AddWearablesToCatalog(wearables);
                    result.AddRange(wearables);
                }

                sourceToAwait.TrySetResult(result);
            }
            else
                result = (List<WearableItem>)await sourceToAwait.Task;

            ct.ThrowIfCancellationRequested();

            return result.FirstOrDefault(x => x.id == newWearableId);
        }

        private List<WearableItem> ValidateWearables(
            IEnumerable<WearableWithEntityResponseDto.ElementDto> wearableElements,
            string contentBaseUrl,
            string bundlesBaseUrl)
        {
            List<WearableItem> wearables = new ();

            foreach (var item in wearableElements)
            {
                WearableWithEntityResponseDto.ElementDto.EntityDto entity = item.entity;
                WearableWithEntityResponseDto.ElementDto.EntityDto.MetadataDto metadata = entity.metadata;

                if (IsInvalidWearable(metadata))
                    continue;

                try
                {
                    WearableItem wearable = item.ToWearableItem(contentBaseUrl, bundlesBaseUrl);
                    wearables.Add(wearable);
                }
                catch (Exception e) { Debug.LogException(e); }
            }

            return wearables;
        }

        private void MapLambdasDataIntoWearableItem(IList<WearableItem> wearablesFromLambdas)
        {
            var invalidWearablesIndices = ListPool<int>.Get();

            for (var i = 0; i < wearablesFromLambdas.Count; i++)
            {
                var wearable = wearablesFromLambdas[i];

                if (IsInvalidWearable(wearable))
                {
                    invalidWearablesIndices.Add(i);
                    continue;
                }

                try
                {
                    foreach (var representation in wearable.data.representations)
                    {
                        foreach (var representationContent in representation.contents)
                            representationContent.hash = representationContent.url[(representationContent.url.LastIndexOf('/') + 1)..];
                    }

                    string thumbnail = wearable.thumbnail ?? "";
                    int index = thumbnail.LastIndexOf('/');
                    string newThumbnail = thumbnail[(index + 1)..];
                    string newBaseUrl = thumbnail[..(index + 1)];
                    wearable.thumbnail = newThumbnail;
                    wearable.baseUrl = string.IsNullOrEmpty(newBaseUrl) ? $"{catalyst.contentUrl}/contents/" : newBaseUrl;
                    wearable.baseUrlBundles = ASSET_BUNDLES_URL_ORG;
                    wearable.emoteDataV0 = null;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    invalidWearablesIndices.Add(i);
                }
            }

            for (var i = 0; i < invalidWearablesIndices.Count; i++)
            {
                int invalidWearablesIndex = invalidWearablesIndices[i] - i;
                wearablesFromLambdas.RemoveAt(invalidWearablesIndex);
            }

            ListPool<int>.Release(invalidWearablesIndices);
        }

        private static bool IsInvalidWearable(WearableItem item)
        {
            if (string.IsNullOrEmpty(item.id))
            {
                Debug.LogError("Wearable is invalid: id is null");
                return true;
            }

            if (item.data.representations == null)
            {
                Debug.LogError($"Wearable ${item.id} is invalid: data.representation is null");
                return true;
            }

            foreach (var dataRepresentation in item.data.representations)
            {
                foreach (var representationContent in dataRepresentation.contents)
                {
                    if (string.IsNullOrEmpty(representationContent.url))
                    {
                        Debug.LogError("Wearable is invalid: representation content URL is null");
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsInvalidWearable(WearableWithEntityResponseDto.ElementDto.EntityDto.MetadataDto metadata)
        {
            if (metadata == null)
            {
                Debug.LogError("Wearable is invalid: metadata is null");
                return true;
            }

            if (string.IsNullOrEmpty(metadata.id))
            {
                Debug.LogError("Wearable is invalid: id is null");
                return true;
            }

            if (metadata.data.representations == null)
            {
                Debug.LogError($"Wearable ${metadata.id} is invalid: data.representation is null");
                return true;
            }

            foreach (var representation in metadata.data.representations)
            {
                foreach (string content in representation.contents)
                {
                    if (string.IsNullOrEmpty(content))
                    {
                        Debug.LogError("Wearable is invalid: representation content URL is null");
                        return true;
                    }
                }
            }

            return false;
        }

        private static (string paramName, string paramValue)[] GetWearablesUrlParams(IEnumerable<string> wearableIds) =>
            wearableIds.Select(id => ("wearableId", id)).ToArray();
    }
}
