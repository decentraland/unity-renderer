using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine.Pool;

namespace DCLServices.EmotesCatalog
{
    public class EmotesBatchRequest : IEmotesRequestSource
    {
        [Serializable]
        public class OwnedEmotesRequestDto
        {
            // https://decentraland.github.io/catalyst-api-specs/#tag/Lambdas/operation/getEmotes

            [Serializable]
            public class EmoteRequestDto
            {
                public string urn;
                public int amount;
                public IndividualData[] individualData;

                [Serializable]
                public struct IndividualData
                {
                    // extended urn
                    public string id;
                }
            }

            public EmoteRequestDto[] elements;
            public int totalAmount;
            public int pageSize;
            public int pageNum;
        }

        public event Action<IReadOnlyList<WearableItem>> OnEmotesReceived;
        public event IEmotesRequestSource.OwnedEmotesReceived OnOwnedEmotesReceived;
        public event EmoteRejectedDelegate OnEmoteRejected;

        private readonly ILambdasService lambdasService;
        private readonly ICatalyst catalyst;
        private readonly HashSet<string> pendingRequests = new ();
        private readonly CancellationTokenSource cts;
        private readonly BaseVariable<FeatureFlag> featureFlags;
        private UniTaskCompletionSource<IReadOnlyList<WearableItem>> lastRequestSource;

        private string assetBundlesUrl => featureFlags.Get().IsFeatureEnabled("ab-new-cdn") ? "https://ab-cdn.decentraland.org/" : "https://content-assets-as-bundle.decentraland.org/";

        public EmotesBatchRequest(ILambdasService lambdasService, IServiceProviders serviceProviders, BaseVariable<FeatureFlag> featureFlags)
        {
            this.featureFlags = featureFlags;
            this.lambdasService = lambdasService;
            catalyst = serviceProviders.catalyst;
            cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }

        public void RequestOwnedEmotes(string userId)
        {
            RequestOwnedEmotesAsync(userId).Forget();
        }

        private async UniTask RequestOwnedEmotesAsync(string userId)
        {
            var url = $"{catalyst.lambdasUrl}/users/{userId}/emotes";

            var requestedEmotes = new List<OwnedEmotesRequestDto.EmoteRequestDto>();

            if (!await FullListEmoteFetch(userId, url, requestedEmotes))
                return;

            PoolUtils.ListPoolRent<string> tempList = PoolUtils.RentList<string>();
            List<string> emoteUrns = tempList.GetList();

            var urnToAmountMap = new Dictionary<string, int>();
            var idToExtendedUrn = new Dictionary<string, string>();

            foreach (OwnedEmotesRequestDto.EmoteRequestDto emoteRequestDto in requestedEmotes)
            {
                emoteUrns.Add(emoteRequestDto.urn);
                urnToAmountMap[emoteRequestDto.urn] = emoteRequestDto.amount;
                idToExtendedUrn[emoteRequestDto.urn] = emoteRequestDto.individualData[0].id;
            }

            IReadOnlyList<WearableItem> emotes = await FetchEmotes(emoteUrns, urnToAmountMap);

            tempList.Dispose();

            OnOwnedEmotesReceived?.Invoke(emotes, userId, idToExtendedUrn);
        }

        // This recursiveness is horrible, we should add proper pagination
        private async UniTask<bool> FullListEmoteFetch(string userId, string url, List<OwnedEmotesRequestDto.EmoteRequestDto> requestedEmotes)
        {
            var requestedCount = 0;
            var totalEmotes = 99999;
            var pageNum = 1;

            while (requestedCount < totalEmotes)
            {
                (string name, string value)[] queryParams = new List<(string name, string value)>
                {
                    ("pageNum", pageNum.ToString()),
                }.ToArray();

                (OwnedEmotesRequestDto response, bool success) result = await lambdasService.GetFromSpecificUrl<OwnedEmotesRequestDto>(url, url,
                    cancellationToken: cts.Token, urlEncodedParams: queryParams);

                if (!result.success) throw new Exception($"Fetching owned wearables failed! {url}\nAddress: {userId}");

                if (result.response.elements.Length <= 0 && requestedCount <= 0)
                {
                    OnOwnedEmotesReceived?.Invoke(new List<WearableItem>(), userId, new Dictionary<string, string>());
                    return false;
                }

                requestedCount += result.response.elements.Length;
                totalEmotes = result.response.totalAmount;
                pageNum++;
                requestedEmotes.AddRange(result.response.elements);
            }

            return true;
        }

        public void RequestEmote(string emoteId)
        {
            RequestWearableBatchAsync(emoteId).Forget();
        }

        private async UniTask RequestWearableBatchAsync(string id)
        {
            pendingRequests.Add(id);
            lastRequestSource ??= new UniTaskCompletionSource<IReadOnlyList<WearableItem>>();
            UniTaskCompletionSource<IReadOnlyList<WearableItem>> sourceToAwait = lastRequestSource;

            // we wait for the latest update possible so we buffer all requests into one
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cts.Token);

            IReadOnlyList<WearableItem> result;

            if (pendingRequests.Count > 0)
            {
                lastRequestSource = null;

                List<string> tempList = ListPool<string>.Get();
                tempList.AddRange(pendingRequests);
                pendingRequests.Clear();

                result = await FetchEmotes(tempList);
                ListPool<string>.Release(tempList);
                sourceToAwait.TrySetResult(result);
                OnEmotesReceived?.Invoke(result);
            }
            else
                await sourceToAwait.Task;
        }

        private async UniTask<IReadOnlyList<WearableItem>> FetchEmotes(
            IReadOnlyCollection<string> ids,
            Dictionary<string, int> urnToAmountMap = null)
        {
            // the copy of the list is intentional
            var request = new LambdasEmotesCatalogService.WearableRequest { pointers = new List<string>(ids) };
            var url = $"{catalyst.contentUrl}entities/active";

            (EmoteEntityDto[] response, bool success) response = await lambdasService.PostFromSpecificUrl<EmoteEntityDto[], LambdasEmotesCatalogService.WearableRequest>(
                url, url, request, cancellationToken: cts.Token);

            if (!response.success) throw new Exception($"Fetching wearables failed! {url}\n{string.Join("\n", request.pointers)}");

            HashSet<string> receivedIds = HashSetPool<string>.Get();

            IEnumerable<WearableItem> wearables = response.response.Select(dto =>
            {
                var contentUrl = $"{catalyst.contentUrl}contents/";
                var wearableItem = dto.ToWearableItem(contentUrl);

                if (urnToAmountMap != null && urnToAmountMap.TryGetValue(dto.metadata.id, out int amount))
                    wearableItem.amount = amount;

                wearableItem.baseUrl = contentUrl;
                wearableItem.baseUrlBundles = assetBundlesUrl;
                return wearableItem;
            });

            foreach (WearableItem wearableItem in wearables)
                receivedIds.Add(wearableItem.id);

            foreach (string id in ids)
                if (!receivedIds.Contains(id))
                    OnEmoteRejected?.Invoke(id, "Empty response from content server");

            HashSetPool<string>.Release(receivedIds);
            return wearables.ToList();
        }
    }
}
