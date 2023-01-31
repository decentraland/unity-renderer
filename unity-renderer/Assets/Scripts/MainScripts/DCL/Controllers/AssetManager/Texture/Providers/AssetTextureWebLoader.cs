using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Downloads Texture by Web URL
    /// </summary>
    public class AssetTextureWebLoader : ITextureAssetProvider
    {
        public async UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken = default)
        {
            UnityWebRequest uwr = await Environment.i.platform.webRequest.GetTexture(url, cancellationToken: cancellationToken);

            if (uwr.result != UnityWebRequest.Result.Success)
                throw new Exception($"Texture promise failed: {uwr.error}");

            return DownloadHandlerTexture.GetContent(uwr);
        }
    }
}
