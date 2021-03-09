using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTGifAsset : INFTAsset
    {
        public bool isHQ => hqGifPromise != null;
        public int hqResolution { get; }
        public ITexture previewAsset => previewGif;
        public ITexture hqAsset => hqGifPromise?.asset;

        private AssetPromise_Gif hqGifPromise;

        private readonly GifPlayer gifPlayer;
        private readonly Asset_Gif previewGif;

        public NFTGifAsset(Asset_Gif previewGif, int hqResolution, GifPlayer gifPlayer)
        {
            this.previewGif = previewGif;
            this.hqResolution = hqResolution;
            this.gifPlayer = gifPlayer;
        }

        public void Dispose()
        {
            if (hqGifPromise != null)
            {
                AssetPromiseKeeper_Gif.i.Forget(hqGifPromise);
                hqGifPromise = null;
            }
        }

        public void FetchAndSetHQAsset(string url, Action onSuccess, Action onFail)
        {
            hqGifPromise = new AssetPromise_Gif(url);

            hqGifPromise.OnSuccessEvent += (asset) =>
            {
                gifPlayer?.SetGif(asset);
                onSuccess?.Invoke();
            };
            hqGifPromise.OnFailEvent += (asset) =>
            {
                hqGifPromise = null;
                onFail?.Invoke();
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