using Cysharp.Threading.Tasks;
using MainScripts.DCL.AssetsEmbedment.Runtime;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    public class EmbeddedAssetBundleProvider : AssetBundleWebRequestBasedProvider, IAssetBundleProvider
    {
        private Service<IWebRequestController> webRequestController;

        public async UniTask<AssetBundle> GetAssetBundleAsync(string contentUrl, string hash, CancellationToken cancellationToken)
        {
            var streamingPath = GetUrl(hash);
            // For WebGL the only way to load assets from `StreamingFolder` is via `WebRequest`
            using var webRequest = webRequestController.Ref.GetAssetBundle(streamingPath, requestAttemps: 1, disposeOnCompleted: false);
            return await FromWebRequestAsync(webRequest, streamingPath, cancellationToken);
        }

        private static string GetUrl(string hash)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return $"file://{Application.streamingAssetsPath}/{EmbeddedWearablesPath.VALUE}/{hash}";
#endif
            return $"{Application.streamingAssetsPath}/{EmbeddedWearablesPath.VALUE}/{hash}";
        }
    }
}
