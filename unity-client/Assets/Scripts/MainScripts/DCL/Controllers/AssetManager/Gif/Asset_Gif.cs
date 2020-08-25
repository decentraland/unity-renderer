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
        public enum MaxSize
        {
            DONT_RESIZE = -1,
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048
        }

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
                    return gifTextures[currentTextureIdx].m_texture2d;

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
        private MaxSize maxSize;

        public event Action<ITexture, AssetPromise_Texture> OnSuccessEvent;
        public event Action OnFailEvent;

        public Asset_Gif(string url, MaxSize maxSize, Action<ITexture, AssetPromise_Texture> OnSuccess, Action OnFail = null)
        {
            this.url = url;
            this.maxSize = maxSize;
            this.OnSuccessEvent = OnSuccess;
            this.OnFailEvent = OnFail;
        }

        public void OverrideGifTextures(List<UniGif.GifTexture> newTextures)
        {
            if (newTextures == null || newTextures.Count == 0) return;

            OnGifLoaded(newTextures, 0, newTextures[0].m_texture2d.width, newTextures[0].m_texture2d.height);
        }

        public IEnumerator Load()
        {
            if (isLoaded)
                Dispose();

#if !UNITY_EDITOR && UNITY_WEBGL
            yield return DCL.GIFProcessingBridge.i.RequestGIFProcessor(url, OverrideGifTextures);
#else
            byte[] bytes = null;

            yield return Utils.FetchAsset(url, UnityWebRequest.Get(url), (request) => { bytes = request.downloadHandler.data; });

            if (bytes == null)
            {
                OnFailEvent?.Invoke();
                yield break;
            }

            yield return UniGif.GetTextureListCoroutine(bytes, OnGifLoaded);
#endif

            SetMaxTextureSize(maxSize);
            Play();

            OnSuccessEvent?.Invoke(this, null);
        }

        public void Dispose()
        {
            if (!isLoaded)
                return;

            Cleanup();
        }

        void Cleanup()
        {
            for (int i = 0; i < gifTextures.Count; i++)
            {
                if (gifTextures[i].m_texture2d)
                    UnityEngine.Object.Destroy(gifTextures[i].m_texture2d);
            }

            gifTextures.Clear();
            gifTextures = null;

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

        public void SetMaxTextureSize(MaxSize size)
        {
            if (!isLoaded)
                return;

            if (size == MaxSize.DONT_RESIZE)
                return;

            int texturesCount = gifTextures.Count;
            for (int i = 0; i < texturesCount; i++)
            {
                TextureHelpers.EnsureTexture2DMaxSize(ref gifTextures[i].m_texture2d, (int)size);
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