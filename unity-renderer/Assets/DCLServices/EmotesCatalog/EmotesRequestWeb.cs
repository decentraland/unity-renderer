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
        private string assetBundlesUrl = "https://content-assets-as-bundle.decentraland.org/";

        private readonly ILambdasService lambdasService;
        private readonly ICatalyst catalyst;
        private readonly List<string> pendingRequests = new ();
        private readonly CancellationTokenSource cts;

        private UniTaskCompletionSource<IReadOnlyList<WearableItem>> lastRequestSource;

        public event Action<WearableItem[]> OnEmotesReceived;
        public event EmoteRejectedDelegate OnEmoteRejected;
        public event Action<WearableItem[], string> OnOwnedEmotesReceived;


        public EmotesRequestWeb(ILambdasService lambdasService, IServiceProviders serviceProviders, BaseVariable<FeatureFlag> featureFlags)
        {
            this.lambdasService = lambdasService;
            this.catalyst = serviceProviders.catalyst;
            assetBundlesUrl = featureFlags.Get().IsFeatureEnabled("ab-new-cdn") ? "https://ab-cdn.decentraland.org/" : "https://content-assets-as-bundle.decentraland.org/";
            cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }

        public void RequestOwnedEmotes(string userId)
        {
            throw new NotImplementedException();
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
                var request = new EmotesCatalogService.WearableRequest { pointers = new List<string>(pendingRequests) };
                var url = $"{catalyst.contentUrl}entities/active";

                pendingRequests.Clear();

                var response = await lambdasService.PostFromSpecificUrl<EmoteEntityDto[], EmotesCatalogService.WearableRequest>(url, url, request, cancellationToken: cts.Token);

                if (response.success)
                    result.AddRange( response.response.Select(dto =>
                    {
                        var contentUrl = $"{catalyst.contentUrl}contents/";
                        var wearableItem = dto.ToWearableItem(contentUrl);
                        wearableItem.baseUrl = contentUrl;
                        wearableItem.baseUrlBundles = assetBundlesUrl;
                        return wearableItem;
                    }));
                else
                    throw new Exception($"Fetching wearables failed! {url}\n{string.Join("\n",request.pointers)}");

                lastRequestSource.TrySetResult(result);
            }
            else
                result = (List<WearableItem>)await lastRequestSource.Task;

            OnEmotesReceived?.Invoke(result.ToArray());
        }
    }
}
