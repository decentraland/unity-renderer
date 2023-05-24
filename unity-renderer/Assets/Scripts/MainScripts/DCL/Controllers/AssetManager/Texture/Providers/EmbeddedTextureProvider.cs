using Cysharp.Threading.Tasks;
using DCL.Providers;
using System.Threading;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Provides embeded textures
    /// </summary>
    public class EmbeddedTextureProvider : ITextureAssetProvider
    {
        private readonly IAddressableResourceProvider resourceProvider;

        public EmbeddedTextureProvider(IAddressableResourceProvider resourceProvider)
        {
            this.resourceProvider = resourceProvider;
        }

        public async UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken = default)
        {
            int lastSlash = url.LastIndexOf('/');
            string hash = lastSlash > -1 ? url.Remove(0, lastSlash + 1) : url;

            return await resourceProvider.GetAddressable<Texture2D>(hash, cancellationToken);
        }
    }
}
