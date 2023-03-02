using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    public interface IAssetBundleResolver : IService
    {
        UniTask<AssetBundle> GetAssetBundleAsync(AssetSource permittedSources, string contentUrl,
            string hash, CancellationToken cancellationToken = default);

        void IService.Initialize() { }

        void IDisposable.Dispose() { }
    }
}
