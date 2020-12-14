using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTImageAsset : INFTAsset
    {
        public bool isHQ => hqAsset != null;
        public int hqResolution { private set; get; }
        public Action<Texture2D> UpdateTextureCallback { set; get; }
        public ITexture previewAsset => previewTexture;
        public ITexture hqAsset => hqTexture != null ? hqTexture.asset : null;

        private AssetPromise_Texture hqTexture;
        private ITexture previewTexture;

        public NFTImageAsset(ITexture previewTexture, int hqResolution)
        {
            this.previewTexture = previewTexture;
            this.hqResolution = hqResolution;
        }

        public void Dispose()
        {
            if (hqTexture == null)
                return;

            AssetPromiseKeeper_Texture.i.Forget(hqTexture);
            hqTexture = null;
            UpdateTextureCallback = null;
        }

        public void FetchAndSetHQAsset(string url, Action onSuccess, Action onFail)
        {
            hqTexture = new AssetPromise_Texture(url);
            AssetPromiseKeeper_Texture.i.Keep(hqTexture);

            hqTexture.OnSuccessEvent += (asset) =>
            {
                UpdateTextureCallback?.Invoke(asset.texture);
                onSuccess?.Invoke();
            };
            hqTexture.OnFailEvent += (asset) =>
            {
                hqTexture = null;
                onFail?.Invoke();
            };
        }

        public void RestorePreviewAsset()
        {
            if (hqTexture != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(hqTexture);
                hqTexture = null;
            }

            UpdateTextureCallback?.Invoke(previewAsset.texture);
        }
    }
}