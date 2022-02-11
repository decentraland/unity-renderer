using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTImageAsset : INFTAsset
    {
        public bool isHQ => hqTexture != null;
        public int hqResolution { private set; get; }
        public ITexture previewAsset { get; }
        public ITexture hqAsset => hqTexture?.asset;

        private AssetPromise_Texture hqTexture;
        private Action<Texture2D> textureUpdateCallback;

        public NFTImageAsset(Asset_Texture previewTexture, int hqResolution, Action<Texture2D> textureUpdateCallback)
        {
            previewAsset = previewTexture;
            this.hqResolution = hqResolution;
            this.textureUpdateCallback = textureUpdateCallback;
        }

        public void Dispose()
        {
            if (hqTexture == null)
                return;

            AssetPromiseKeeper_Texture.i.Forget(hqTexture);
            hqTexture = null;
            textureUpdateCallback = null;
        }

        public void FetchAndSetHQAsset(string url, Action onSuccess, Action<Exception> onFail)
        {
            hqTexture = new AssetPromise_Texture(url);

            hqTexture.OnSuccessEvent += asset =>
            {
                textureUpdateCallback?.Invoke(asset.texture);
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

            textureUpdateCallback?.Invoke(previewAsset.texture);
        }
    }
}