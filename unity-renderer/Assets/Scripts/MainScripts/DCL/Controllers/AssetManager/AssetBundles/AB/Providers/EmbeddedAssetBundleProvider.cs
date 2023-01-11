using Cysharp.Threading.Tasks;
using System.IO;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    public class EmbeddedAssetBundleProvider : AssetBundleWebRequestBasedProvider, IAssetBundleProvider
    {
        private Service<IWebRequestController> webRequestController;

        public async UniTask<AssetBundle> GetAssetBundleAsync(string contentUrl, string hash, CancellationToken cancellationToken)
        {
            var streamingPath = Path.Combine(Application.streamingAssetsPath, hash);
            // For WebGL the only way to load assets from `StreamingFolder` is via `WebRequest`
            using var webRequest = webRequestController.Ref.GetAssetBundle(streamingPath, requestAttemps: 1);
            return await FromWebRequestAsync(webRequest, streamingPath, cancellationToken);
        }
    }
}
