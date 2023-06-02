using Cysharp.Threading.Tasks;
using MainScripts.DCL.AssetsEmbedment.Runtime;
using System.Threading;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Provides textures from "Resources"
    /// </summary>
    public class EmbeddedTextureProvider : ITextureAssetProvider
    {
        public async UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken = default)
        {
            var lastSlash = url.LastIndexOf('/');
            var hash = lastSlash > -1 ? url.Remove(0, lastSlash + 1) : url;
            var result = await Resources.LoadAsync<Texture2D>(EmbeddedTextureResourcesPath.VALUE + "/" + hash).WithCancellation(cancellationToken);
            return (Texture2D)result;
        }
    }
}
