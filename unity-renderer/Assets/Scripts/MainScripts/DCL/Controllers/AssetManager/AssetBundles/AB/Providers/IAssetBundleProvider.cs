using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    public interface IAssetBundleProvider
    {
        UniTask<AssetBundle> GetAssetBundleAsync(string contentUrl, string hash, CancellationToken cancellationToken);
    }
}
