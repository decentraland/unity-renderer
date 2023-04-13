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
            using var asyncOp = Environment.i.platform.webRequest.GetTexture(url, disposeOnCompleted: false);

            await asyncOp.WithCancellation(cancellationToken);

            if (!asyncOp.isSucceeded)
            {
                var webRequestError = asyncOp.webRequest.error;
                throw new Exception($"Texture promise failed: {webRequestError}");
            }

            Texture2D texture2D = new Texture2D(1, 1);
            try
            {
                texture2D.LoadImage(asyncOp.webRequest.downloadHandler.data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Texture promise failed: {e}");
                return null;
            }

            return texture2D;
        }

    }
}
