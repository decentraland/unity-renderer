using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers.Gif
{
    public class DCLGif : IDisposable
    {
        public event Action<Texture2D> OnFrameTextureChanged;

        public bool isLoaded { get { return gifTextures != null; } }
        public bool isPlaying { get; private set; }
        public int textureWidth { get; private set; }
        public int textureHeight { get; private set; }
        public Texture2D texture
        {
            get
            {
                if (isLoaded)
                    return gifTextures[currentTextureIdx].m_texture2d;
                return null;
            }
        }

        private List<UniGif.GifTexture> gifTextures;

        private int currentLoopCount;
        private float currentTimeDelay;
        private int currentTextureIdx = 0;
        private int maxLoopCount = 0;


        public IEnumerator Load(byte[] bytes, Action onFinish)
        {
            if (isLoaded)
            {
                Dispose();
            }

            yield return UniGif.GetTextureListCoroutine(bytes, OnGifLoaded, FilterMode.Bilinear, TextureWrapMode.Clamp);
            onFinish?.Invoke();
        }

        public void Dispose()
        {
            if (isLoaded)
            {
                for (int i = 0; i < gifTextures.Count; i++)
                {
                    if (gifTextures[i].m_texture2d)
                        UnityEngine.Object.Destroy(gifTextures[i].m_texture2d);
                }
                gifTextures.Clear();
                gifTextures = null;
            }
        }

        public void Play()
        {
            if (!isLoaded)
            {
                return;
            }

            isPlaying = true;

            currentLoopCount = 0;
            currentTextureIdx = 0;
            currentTimeDelay = gifTextures[currentTextureIdx].m_delaySec;
            OnFrameTextureChanged?.Invoke(texture);
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
            textureWidth = width;
            textureHeight = height;
        }
    }
}