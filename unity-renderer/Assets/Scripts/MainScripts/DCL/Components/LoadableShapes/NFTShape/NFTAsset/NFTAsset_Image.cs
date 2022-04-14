using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTAsset_Image : INFTAsset
    {
        private const int RESOLUTION_HQ = 1024;
        public bool isHQ => hqTexture != null;
        public ITexture previewAsset { get; }
        public ITexture hqAsset => hqTexture?.asset;

        private AssetPromise_Texture hqTexture;
        public event Action<Texture2D> OnTextureUpdate;

        public NFTAsset_Image(Asset_Texture previewTexture = null)
        {
            previewAsset = previewTexture;
        }

        public void Dispose()
        {
            if (hqTexture == null)
                return;

            AssetPromiseKeeper_Texture.i.Forget(hqTexture);
            hqTexture = null;
            OnTextureUpdate = null;
        }

        public void FetchAndSetHQAsset(string url, Action onSuccess, Action<Exception> onFail)
        {
            string finalUrl = $"{url}=s{RESOLUTION_HQ}";
            hqTexture = new AssetPromise_Texture($"{finalUrl}");

            hqTexture.OnSuccessEvent += asset =>
            {
                OnTextureUpdate?.Invoke(asset.texture);
                onSuccess?.Invoke();
            };
            hqTexture.OnFailEvent += (asset, error) =>
            {
                hqTexture = null;
                onFail?.Invoke(error);
            };

            AssetPromiseKeeper_Texture.i.Keep(hqTexture);
        }

        public void RestorePreviewAsset()
        {
            if (hqTexture != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(hqTexture);
                hqTexture = null;
            }

            if (previewAsset == null)
                return;

            OnTextureUpdate?.Invoke(previewAsset.texture);
        }
    }
}