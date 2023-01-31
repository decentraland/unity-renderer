using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Providers
{
    public abstract class AssetBundleWebRequestBasedProvider
    {
        protected static async UniTask<AssetBundle> FromWebRequestAsync(UniTask<UnityWebRequest> requestTask, string url)
        {
             UnityWebRequest uwr = await requestTask;

            if (uwr == null)
                throw new AssetBundleException($"Operation is disposed. Url: {url}");

            if (uwr.result != UnityWebRequest.Result.Success)
                throw new AssetBundleException($"Request failed {uwr.error}. Url: {url}");

            return DownloadHandlerAssetBundle.GetContent(uwr);
        }
    }
}
