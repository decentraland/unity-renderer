using Cysharp.Threading.Tasks;
using System.IO;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    /// <summary>
    /// Local Asset Bundles support, if you have the asset bundles in that folder, we are going to load them from there.
    /// This is a debug feature
    /// </summary>
    public class EditorAssetBundleProvider : IAssetBundleProvider
    {
        public UniTask<AssetBundle> GetAssetBundleAsync(string contentUrl, string hash, CancellationToken cancellationToken)
        {
            string localUrl = Application.dataPath + "/../AssetBundles/" + hash;

            return File.Exists(localUrl)
                ? UniTask.FromResult(AssetBundle.LoadFromFile(localUrl))
                : UniTask.FromResult<AssetBundle>(null);
        }
    }
}
