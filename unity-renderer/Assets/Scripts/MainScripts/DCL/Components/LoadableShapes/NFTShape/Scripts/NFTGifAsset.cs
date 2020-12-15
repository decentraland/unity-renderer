using System;
using System.Collections;
using DCL;
using DCL.Controllers.Gif;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTGifAsset : INFTAsset
    {
        public bool isHQ => hqAsset != null;
        public int hqResolution { private set; get; }
        public Action<Texture2D> UpdateTextureCallback { set; get; }
        public ITexture previewAsset => previewGif;
        public ITexture hqAsset => hqTexture != null ? hqTexture : null;

        private Asset_Gif hqTexture;
        private Asset_Gif previewGif;
        private Coroutine loadRoutine;

        public NFTGifAsset(Asset_Gif previewGif, int hqResolution)
        {
            this.previewGif = previewGif;
            this.hqResolution = hqResolution;
        }

        public void Dispose()
        {
            StopGifLoad();

            if (hqTexture != null)
            {
                hqTexture.Dispose();
                hqTexture = null;
            }

            UpdateTextureCallback = null;
        }

        public void FetchAndSetHQAsset(string url, Action onSuccess, Action onFail)
        {
            hqTexture = new Asset_Gif(url,
                (downloadedTex, texturePromise) =>
                {
                    StopGif(previewGif);
                    StartGif(hqTexture);
                    onSuccess?.Invoke();
                },
                () =>
                {
                    hqTexture = null;
                    onFail?.Invoke();
                });
            loadRoutine = CoroutineStarter.Start(LoadGif(hqTexture));
        }

        public void RestorePreviewAsset()
        {
            StopGifLoad();

            if (hqTexture != null)
            {
                hqTexture.Dispose();
                hqTexture = null;
            }

            StartGif(previewGif);
        }

        private void StartGif(Asset_Gif gif)
        {
            if (UpdateTextureCallback != null)
            {
                gif.OnFrameTextureChanged -= UpdateTextureCallback;
                gif.OnFrameTextureChanged += UpdateTextureCallback;
            }

            gif.Play(false);
        }

        private void StopGif(Asset_Gif gif)
        {
            if (UpdateTextureCallback != null)
            {
                gif.OnFrameTextureChanged -= UpdateTextureCallback;
            }

            gif.Stop();
        }

        private IEnumerator LoadGif(Asset_Gif gif)
        {
            yield return gif.Load();
            loadRoutine = null;
        }

        private void StopGifLoad()
        {
            if (loadRoutine != null)
            {
                CoroutineStarter.Stop(loadRoutine);
                loadRoutine = null;
            }
        }
    }
}