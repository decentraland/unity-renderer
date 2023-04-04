using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace MainScripts.DCL.AssetsEmbedment.Editor
{
    public static class EditorAssetBundlesDownloader
    {
        public static async UniTask<bool> DownloadAssetBundleWithDependenciesAsync(string baseUrl, string hash, Dictionary<string, byte[]> loadedData)
        {
            if (loadedData.ContainsKey(hash))
                return true;

            var url = $"{baseUrl}{hash}";

            try
            {
                // Passing no extra arguments will bypass caching and crc checks
                var wr = await UnityWebRequest.Get(url).SendWebRequest();

                if (wr.WebRequestSucceded())
                {
                    var data = wr.downloadHandler.data;
                    var assetBundle = await AssetBundle.LoadFromMemoryAsync(data);

                    try
                    {
                        var dependencies = await assetBundle.GetDependenciesAsync(baseUrl, hash, CancellationToken.None);

                        var result = await UniTask.WhenAll(dependencies.Select(d => DownloadAssetBundleWithDependenciesAsync(baseUrl, d, loadedData)));

                        if (result.All(r => r))
                        {
                            loadedData.Add(hash, data);
                            return true;
                        }
                    }
                    finally
                    {
                        if (assetBundle)
                        {
                            assetBundle.Unload(true);
                            Object.DestroyImmediate(assetBundle);
                        }
                    }

                    return false;
                }

                Debug.LogError(wr.error);
            }
            catch (UnityWebRequestException)
            {
                Debug.LogError($"Failed to download AB: {hash}. Url: {url}");
                // ignore for now
                return true;
            }

            return false;
        }
    }
}
