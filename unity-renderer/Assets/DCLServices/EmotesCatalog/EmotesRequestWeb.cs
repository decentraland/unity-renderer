using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DCLServices.EmotesCatalog
{
    public class EmotesRequestWeb : IEmotesRequestSource
    {
        [Serializable]
        public class OwnedEmotesRequestDto
        {
            // https://decentraland.github.io/catalyst-api-specs/#tag/Lambdas/operation/getEmotes

            [Serializable]
            public class EmoteRequestDto
            {
                public string urn;
            }

            public EmoteRequestDto[] elements;
        }

        public event Action<WearableItem[]> OnEmotesReceived;
        public event EmoteRejectedDelegate OnEmoteRejected;
        public event Action<WearableItem[], string> OnOwnedEmotesReceived;

        private readonly ILambdasService lambdasService;
        private readonly ICatalyst catalyst;
        private readonly List<string> pendingRequests = new ();
        private readonly CancellationTokenSource cts;
        private UniTaskCompletionSource<IReadOnlyList<WearableItem>> lastRequestSource;
        private BaseVariable<FeatureFlag> featureFlags;
        private string assetBundlesUrl => featureFlags.Get().IsFeatureEnabled("ab-new-cdn") ? "https://ab-cdn.decentraland.org/" : "https://content-assets-as-bundle.decentraland.org/";

        public EmotesRequestWeb(ILambdasService lambdasService, IServiceProviders serviceProviders, BaseVariable<FeatureFlag> featureFlags)
        {
            this.featureFlags = featureFlags;
            this.lambdasService = lambdasService;
            this.catalyst = serviceProviders.catalyst;
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
            var url = $"{catalyst.lambdasUrl}users/{userId}/emotes";
            var result = await lambdasService.GetFromSpecificUrl<OwnedEmotesRequestDto>(url, url, cancellationToken: cts.Token);

            if (!result.success) throw new Exception($"Fetching owned wearables failed! {url}\nAddress: {userId}");

            var emotes = await FetchEmotes(result.response.elements.Select(e => e.urn));

            OnOwnedEmotesReceived?.Invoke(emotes.ToArray(), userId);
        }

        public void RequestEmote(string emoteId)
        {
            RequestWearableBatchAsync(emoteId).Forget();
        }

        private async UniTask RequestWearableBatchAsync(string id)
        {
            pendingRequests.Add(id);
            lastRequestSource ??= new UniTaskCompletionSource<IReadOnlyList<WearableItem>>();

            // we wait for the latest update possible so we buffer all requests into one
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cts.Token);

            List<WearableItem> result = new List<WearableItem>();

            if (pendingRequests.Count > 0)
            {
                result.AddRange(await FetchEmotes(pendingRequests));

                lastRequestSource.TrySetResult(result);
            }
            else
                result = (List<WearableItem>)await lastRequestSource.Task;

            OnEmotesReceived?.Invoke(result.ToArray());
        }

        private async UniTask<IEnumerable<WearableItem>> FetchEmotes(IEnumerable<string> ids)
        {
            var request = new EmotesCatalogService.WearableRequest { pointers = new List<string>(ids) };
            var url = $"{catalyst.contentUrl}entities/active";

            pendingRequests.Clear();

            var response = await lambdasService.PostFromSpecificUrl<EmoteEntityDto[], EmotesCatalogService.WearableRequest>(url, url, request, cancellationToken: cts.Token);

            if (!response.success) throw new Exception($"Fetching wearables failed! {url}\n{string.Join("\n", request.pointers)}");

            var wearables = response.response.Select(dto =>
            {
                var contentUrl = $"{catalyst.contentUrl}contents/";
                var wearableItem = dto.ToWearableItem(contentUrl);
                wearableItem.baseUrl = contentUrl;
                wearableItem.baseUrlBundles = assetBundlesUrl;
                return wearableItem;
            });

            return wearables;

        }
    }
}
