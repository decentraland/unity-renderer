using System;
using DCL;
using UnityEngine;

internal class ThumbnailHandler : IDisposable
{
    public Texture2D texture { private set; get; }

    AssetPromise_Texture texturePromise = null;

    public void FetchThumbnail(string url, Action<Texture2D> onSuccess, Action onFail)
    {
        if (!(texture is null))
        {
            onSuccess?.Invoke(texture);
        }
        else if (string.IsNullOrEmpty(url))
        {
            onFail?.Invoke();
        }
        else if (texturePromise is null)
        {
            texturePromise = new AssetPromise_Texture(url, storeTexAsNonReadable: false);
            texturePromise.OnSuccessEvent += textureAsset =>
            {
                texture = textureAsset.texture;
                onSuccess?.Invoke(texture);
            };
            texturePromise.OnFailEvent += textureAsset =>
            {
                texturePromise = null;
                onFail?.Invoke();
            };
            AssetPromiseKeeper_Texture.i.Keep(texturePromise);
        }
    }

    public void Dispose()
    {
        if (texturePromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(texturePromise);
            texturePromise = null;
        }
    }
}
