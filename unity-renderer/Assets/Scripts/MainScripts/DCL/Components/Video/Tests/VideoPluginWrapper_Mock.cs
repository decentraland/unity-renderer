using System;
using System.Collections;
using DCL.Components.Video.Plugin;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tests
{
    /// <summary>
    /// VideoPluginWrapper Mock class.
    ///
    /// Not using NSubstitute to generate this because is too complex.
    /// When running integration tests of the video plugin, it's safer to use a close copycat.
    /// </summary>
    public class VideoPluginWrapper_Mock : IVideoPluginWrapper, IDisposable
    {
        private string id;
        private bool wasRemoved;
        private float volume;
        private float time;
        private VideoState state = VideoState.NONE;

        private Coroutine coroutineDelayedReady;
        private Coroutine coroutinePlay;
        private Texture2D texture = null;

        public void Dispose()
        {
            if (coroutineDelayedReady != null)
            {
                CoroutineStarter.Stop(coroutineDelayedReady);
                coroutineDelayedReady = null;
            }

            if (coroutinePlay != null)
            {
                CoroutineStarter.Stop(coroutinePlay);
                coroutinePlay = null;
            }
        }

        public void Create(string id, string url, bool useHls)
        {
            this.id = id;
            Assert.IsTrue(coroutineDelayedReady == null, "delayedReady should be null at this stage!");
            state = VideoState.LOADING;
            coroutineDelayedReady = CoroutineStarter.Start(GetReadyAfterDelay());
            texture = new Texture2D(1, 1);
        }
        
        public void Create(string id, string url, VideoType type)
        {
            Create(id, url, type == VideoType.Hls);
        }

        /// <summary>
        /// This small method simulates what happens in the plugin. Ready state can get delayed.
        /// </summary>
        /// <returns></returns>
        IEnumerator GetReadyAfterDelay()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            state = VideoState.READY;
        }

        public void Remove(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            Assert.IsFalse(wasRemoved, "Already removed!");
            wasRemoved = true;
            Dispose();
        }

        public void TextureUpdate(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
        }
        public Texture2D PrepareTexture(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            return texture;
        }

        public void Play(string id, float startTime)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            Assert.IsTrue(state != VideoState.NONE &&
                          state != VideoState.ERROR &&
                          state != VideoState.LOADING, $"Trying to play video when is in invalid state! ({state})");

            state = VideoState.PLAYING;

            if (startTime > 0)
                time = startTime;

            coroutinePlay = CoroutineStarter.Start( PlayUpdate() );
        }

        IEnumerator PlayUpdate()
        {
            while (true)
            {
                if ( state == VideoState.PLAYING )
                    time += Time.deltaTime;
                else
                    break;

                yield return null;
            }
        }

        public void Pause(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            state = VideoState.PAUSED;
        }

        public void SetVolume(string id, float volume)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            this.volume = volume;
        }

        public int GetHeight(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            return 0;
        }

        public int GetWidth(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            return 0;
        }

        public float GetTime(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            return time;
        }

        public float GetDuration(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            return 0;
        }

        public VideoState GetState(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            return state;
        }

        public string GetError(string id)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            return "";
        }

        public void SetTime(string id, float second)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
            this.time = second;
        }

        public void SetPlaybackRate(string id, float playbackRate)
        {
            Assert.AreEqual(this.id, id, "Using ID that was not created!");
        }

        public void SetLoop(string id, bool loop)
        {
        }
    }
}
