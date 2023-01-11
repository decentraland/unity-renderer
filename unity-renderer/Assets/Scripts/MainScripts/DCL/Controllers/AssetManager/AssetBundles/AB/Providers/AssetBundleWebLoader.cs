using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    public class AssetBundleWebLoader : AssetBundleWebRequestBasedProvider, IAssetBundleProvider
    {
        private const string CACHING_FEATURE_FLAG = "ab_caching";

        private Service<IWebRequestController> webRequestController;
        private readonly DataStore_FeatureFlag featureFlags;
        private readonly DataStore_Performance performance;

        private static int maxConcurrentRequests => CommonScriptableObjects.rendererState.Get() ? 30 : 256;
        private bool cachingEnabled => featureFlags.flags.Get().IsFeatureEnabled(CACHING_FEATURE_FLAG);

        public AssetBundleWebLoader(DataStore_FeatureFlag featureFlags, DataStore_Performance performance)
        {
            this.featureFlags = featureFlags;
            this.performance = performance;
        }

        public async UniTask<AssetBundle> GetAssetBundleAsync(string contentUrl, string hash, CancellationToken cancellationToken)
        {
            await WaitForConcurrentRequestsSlot();
            performance.concurrentABRequests.Set(performance.concurrentABRequests.Get() + 1);

            try
            {
                var url = contentUrl + hash;

                using var webRequest = cachingEnabled
                    ? webRequestController.Ref.GetAssetBundle(url, hash: Hash128.Compute(hash), disposeOnCompleted: false)
                    : webRequestController.Ref.GetAssetBundle(url, disposeOnCompleted: false);

                return await FromWebRequestAsync(webRequest, url, cancellationToken);
            }
            finally { performance.concurrentABRequests.Set(performance.concurrentABRequests.Get() - 1);; }
        }

        private UniTask WaitForConcurrentRequestsSlot()
        {
            return UniTask.WaitUntil(() => performance.concurrentABRequests.Get() < maxConcurrentRequests);
        }
    }
}
