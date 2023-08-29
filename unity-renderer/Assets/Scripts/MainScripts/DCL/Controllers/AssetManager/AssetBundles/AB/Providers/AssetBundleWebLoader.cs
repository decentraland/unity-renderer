using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

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
                var hash128 = ComputeHash(contentUrl, hash);

                if (cachingEnabled)
                    AssetResolverLogger.LogVerbose(featureFlags, LogType.Log, $"Asset Bundle {hash} is cached...");

                using var webRequest = cachingEnabled
                    ? webRequestController.Ref.GetAssetBundle(url, hash: hash128, disposeOnCompleted: false)
                    : webRequestController.Ref.GetAssetBundle(url, disposeOnCompleted: false);

                return await FromWebRequestAsync(webRequest, url, cancellationToken);
            }
            finally
            {
                performance.concurrentABRequests.Set(performance.concurrentABRequests.Get() - 1);
            }
        }

        // According to https://adr.decentraland.org/adr/ADR-11
        // we use /vX for versioning so caching system should respect it
        internal static Hash128 ComputeHash(string contentUrl, string hash)
        {
            var hashBuilder = GenericPool<StringBuilder>.Get();
            hashBuilder.Clear();
            hashBuilder.Append(hash);

            var span = contentUrl.AsSpan();

            // content URL always ends with '/'
            int indexOfVersionStart;

            for (indexOfVersionStart = span.Length - 2; span[indexOfVersionStart] != '/'; indexOfVersionStart--) { }

            indexOfVersionStart++;

            if (span[indexOfVersionStart] == 'v')
                hashBuilder.Insert(0, span.Slice(indexOfVersionStart, span.Length - indexOfVersionStart - 1));

            var hash128 = Hash128.Compute(hashBuilder.ToString());
            GenericPool<StringBuilder>.Release(hashBuilder);
            return hash128;
        }

        private UniTask WaitForConcurrentRequestsSlot()
        {
            return UniTask.WaitUntil(() => performance.concurrentABRequests.Get() < maxConcurrentRequests);
        }
    }
}
