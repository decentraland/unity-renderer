using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.AssetManager.Texture;
using System;
using System.Threading;

namespace DCL
{
    public interface ITextureAssetResolver : IService
    {
        UniTask<TextureResponse> GetTextureAsync(AssetSource permittedSources, string url, int maxTextureSize, CancellationToken cancellationToken = default);

        void IService.Initialize() { }

        void IDisposable.Dispose() { }
    }
}
