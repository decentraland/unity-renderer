using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Player for a Gif Asset.
    /// Player will stop if Gif Asset is disposed.
    /// Is not this player responsibility to dispose Gif Asset. Gif Asset should be explicitly disposed.
    /// </summary>
    public class GifPlayer : IDisposable
    {
        public event Action<Texture2D> OnFrameTextureChanged;

        public bool isPlaying { get; private set; } = false;

        private Asset_Gif gifAsset = null;
        private int currentFrameIdx = 0;
        private Coroutine updateRoutine = null;
        private float currentTimeDelay = 0;

        public GifPlayer(Asset_Gif asset)
        {
            SetGif(asset);
        }

        public GifPlayer()
        {
        }

        /// <summary>
        /// Set gif asset for the player
        /// </summary>
        /// <param name="asset">gif asset</param>
        public void SetGif(Asset_Gif asset)
        {
            gifAsset = asset;

            if (isPlaying && IsValidAsset())
            {
                if (currentFrameIdx >= asset.frames.Length)
                {
                    currentFrameIdx = 0;
                }
                SetFrame(currentFrameIdx);
            }
        }

        public void Play(bool reset = false)
        {
            if (reset)
            {
                currentFrameIdx = 0;
                currentTimeDelay = 0;
                Stop();
            }

            isPlaying = true;

            if (updateRoutine == null && gifAsset != null)
            {
                updateRoutine = CoroutineStarter.Start(UpdateRoutine());
            }
        }

        public void Stop()
        {
            isPlaying = false;

            if (updateRoutine != null)
            {
                CoroutineStarter.Stop(updateRoutine);
                updateRoutine = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private IEnumerator UpdateRoutine()
        {
            while (isPlaying)
            {
                yield return WaitForSecondsCache.Get(currentTimeDelay);
                UpdateFrame();
            }
        }

        private bool IsValidAsset()
        {
            return gifAsset?.frames != null && gifAsset.frames.Length > 0;
        }

        private void UpdateFrame()
        {
            if (!IsValidAsset())
            {
                return;
            }

            currentFrameIdx++;

            if (currentFrameIdx >= gifAsset.frames.Length)
            {
                currentFrameIdx = 0;
            }

            SetFrame(currentFrameIdx);
        }

        private void SetFrame(int frameIdx)
        {
            currentTimeDelay = gifAsset.frames[frameIdx].delay;
            OnFrameTextureChanged?.Invoke(gifAsset.frames[frameIdx].texture);
        }
    }
}