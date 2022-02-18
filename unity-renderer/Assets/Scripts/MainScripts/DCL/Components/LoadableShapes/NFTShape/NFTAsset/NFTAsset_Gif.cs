using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTAsset_Gif : INFTAsset
    {
        private const int RESOLUTION_HQ = 512;
        public bool isHQ => hqGifPromise != null;
        public ITexture previewAsset => previewGif;
        public ITexture hqAsset => hqGifPromise?.asset;

        private AssetPromise_Gif hqGifPromise;

        private readonly GifPlayer gifPlayer;
        private readonly Asset_Gif previewGif;
        public event Action<Texture2D> OnTextureUpdate;

        public NFTAsset_Gif(Asset_Gif previewGif)
        {
            this.previewGif = previewGif;
            this.gifPlayer = new GifPlayer(previewGif);

            gifPlayer.Play();
            gifPlayer.OnFrameTextureChanged += OnFrameTextureChanged;
        }

        void OnFrameTextureChanged(Texture2D texture)
        {
            OnTextureUpdate?.Invoke(texture);
        }

        public void Dispose()
        {
            if (hqGifPromise != null)
            {
                AssetPromiseKeeper_Gif.i.Forget(hqGifPromise);
                hqGifPromise = null;
            }

            gifPlayer.Dispose();
        }

        public void FetchAndSetHQAsset(string url, Action onSuccess, Action<Exception> onFail)
        {
            hqGifPromise = new AssetPromise_Gif($"{url}=s{RESOLUTION_HQ}");

            hqGifPromise.OnSuccessEvent += (asset) =>
            {
                gifPlayer?.SetGif(asset);
                onSuccess?.Invoke();
            };
            hqGifPromise.OnFailEvent += (asset, error) =>
            {
                hqGifPromise = null;
                onFail?.Invoke(error);
            };

            AssetPromiseKeeper_Gif.i.Keep(hqGifPromise);
        }

        public void RestorePreviewAsset()
        {
            gifPlayer?.SetGif(previewGif);

            if (hqGifPromise != null)
            {
                AssetPromiseKeeper_Gif.i.Forget(hqGifPromise);
                hqGifPromise = null;
            }
        }
    }
}