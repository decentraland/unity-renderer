using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.AssetManager.Font;
using System;
using System.Threading;

namespace DCL
{
    public interface IFontAssetResolver : IService
    {
        UniTask<FontResponse> GetFontAsync(AssetSource permittedSources, string url, CancellationToken cancellationToken = default);

        void IService.Initialize() { }

        void IDisposable.Dispose() { }
    }
}
