using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Controllers.Gif
{
    public class Asset_Gif : IDisposable, ITexture
    {
        public event Action<Texture2D> OnFrameTextureChanged;

        public bool isLoaded
        {
            get { return gifTextures != null; }
        }

        public bool isPlaying { get; private set; }

        public Texture2D texture
        {
            get
            {
                if (isLoaded)
                {
                    return gifTextures[currentTextureIdx].m_texture2d;
                }

                return null;
            }
        }

        public int width => texture.width;
        public int height => texture.height;

        private List<UniGif.GifTexture> gifTextures;

        private int currentLoopCount;
        private float currentTimeDelay;
        private int currentTextureIdx = 0;
        private int maxLoopCount = 0;

        private Coroutine updateRoutine = null;

        private string url;

        public event Action<ITexture, AssetPromise_Texture> OnSuccessEvent;
        public event Action OnFailEvent;

        private bool processedGIFInJS = false;

        public Asset_Gif(string url, Action<ITexture, AssetPromise_Texture> OnSuccess, Action OnFail = null)
        {
            this.url = url;
            this.OnSuccessEvent = OnSuccess;
            this.OnFailEvent = OnFail;

            KernelConfig.i.EnsureConfigInitialized().Then(config => processedGIFInJS = config.gifSupported);
        }

        public IEnumerator Load()
        {
            if (isLoaded)
                Dispose();

            if (processedGIFInJS)
            {
                bool fetchFailed = false;
                yield return DCL.GIFProcessingBridge.i.RequestGIFProcessor(url,
                    (List<UniGif.GifTexture> newTextures) => // Override textures with JS processed ones
                    {
                        if (newTextures == null || newTextures.Count == 0) return;
                        processedGIFInJS = true;
                        OnGifLoaded(newTextures, 0, newTextures[0].m_texture2d.width, newTextures[0].m_texture2d.height);
                    }, () => fetchFailed = true);

                if (fetchFailed)
                {
                    OnFailEvent?.Invoke();
                    yield break;
                }
            }
            else
            {
                byte[] bytes = null;

                yield return Utils.FetchAsset(url, UnityWebRequest.Get(url), (request) => { bytes = request.downloadHandler.data; });

                if (bytes == null)
                {
                    OnFailEvent?.Invoke();
                    yield break;
                }

                yield return UniGif.GetTextureListCoroutine(bytes, OnGifLoaded);
            }

            Play();

            OnSuccessEvent?.Invoke(this, null);
        }

        public void Dispose()
        {
            if (!isLoaded)
                return;

            Cleanup();
            GIFProcessingBridge.i.DeleteGIF(url);
        }

        void Cleanup()
        {
            Stop();
        }

        public void Play(bool resetTime = false)
        {
            if (!isLoaded)
            {
                return;
            }

            isPlaying = true;

            if (resetTime)
                Stop();

            if (updateRoutine == null)
                updateRoutine = CoroutineStarter.Start(UpdateRoutine());

            OnFrameTextureChanged?.Invoke(texture);
        }

        public void Stop()
        {
            currentLoopCount = 0;
            currentTextureIdx = 0;

            if (gifTextures != null)
                currentTimeDelay = gifTextures[currentTextureIdx].m_delaySec;

            if (updateRoutine != null)
            {
                CoroutineStarter.Stop(updateRoutine);
            }

            updateRoutine = null;
        }

        public IEnumerator UpdateRoutine()
        {
            while (isPlaying)
            {
                yield return WaitForSecondsCache.Get(currentTimeDelay);

                currentTextureIdx++;

                if (currentTextureIdx >= gifTextures.Count)
                {
                    currentLoopCount++;

                    if (maxLoopCount > 0 && currentLoopCount >= maxLoopCount)
                    {
                        isPlaying = false;
                        break;
                    }

                    currentTextureIdx = 0;
                }

                currentTimeDelay = gifTextures[currentTextureIdx].m_delaySec;
                OnFrameTextureChanged?.Invoke(texture);
            }
        }

        private void OnGifLoaded(List<UniGif.GifTexture> gifTextureList, int loopCount, int width, int height)
        {
            if (gifTextureList == null)
                return;

            gifTextures = gifTextureList;
            maxLoopCount = loopCount;
        }
    }
}