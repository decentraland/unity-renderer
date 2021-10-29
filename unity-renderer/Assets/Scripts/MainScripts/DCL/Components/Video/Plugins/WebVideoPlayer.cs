using System;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayer : IDisposable
    {
        public event Action<Texture2D> OnTextureReady;

        public Texture2D texture { private set; get; }
        public float volume { private set; get; }
        public bool playing { get { return shouldBePlaying; } }
        public bool visible { get; set; }
        public bool isError { get; private set; }
        
        private static bool isWebGL1 => SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2;

        private string videoPlayerId;

        private readonly IWebVideoPlayerPlugin plugin;

        private bool initialized = false;
        private bool shouldBePlaying = false;
        private float pausedAtTime = -1;

        public WebVideoPlayer(string id, string url, bool useHls, IWebVideoPlayerPlugin plugin)
        {
            videoPlayerId = id;
            this.plugin = plugin;

            texture = new Texture2D(1, 1);
            plugin.Create(id, url, useHls);
        }

        public void UpdateWebVideoTexture()
        {
            if (isError)
            {
                return;
            }

            switch (plugin.GetState(videoPlayerId))
            {
                case (int)VideoState.ERROR:
                    Debug.unityLogger.logEnabled = true;
                    Debug.LogError(plugin.GetError(videoPlayerId));
                    Debug.unityLogger.logEnabled = false;
                    isError = true;
                    break;
                case (int)VideoState.READY:
                    if (!initialized)
                    {
                        initialized = true;
                        texture.UpdateExternalTexture((IntPtr)plugin.GetTexture(videoPlayerId));
                        texture.Apply();
                        OnTextureReady?.Invoke(texture);
                    }

                    break;
                case (int)VideoState.PLAYING:
                    if (shouldBePlaying && visible)
                    {
                        int width = plugin.GetWidth(videoPlayerId);
                        int height = plugin.GetHeight(videoPlayerId);

                        if (width > 0 && height > 0)
                        {
                            plugin.TextureUpdate(videoPlayerId, isWebGL1);
                        }
                    }

                    break;
            }
        }

        void Log(string s)
        {
            Debug.unityLogger.logEnabled = true;
            Debug.Log("Alex: " + s);
            Debug.unityLogger.logEnabled = false;
        }
        
        public void Play()
        {
            if (isError)
            {
                Log("Tried to play but has error");
                return;
            }

            Log("Playing normally");
            plugin.Play(videoPlayerId, pausedAtTime);
            pausedAtTime = -1;

            shouldBePlaying = true;
        }

        public void Pause()
        {
            if (isError)
            {
                Log($"Tried to pause but has error");
                return;
            }

            Log("Pausing normally");
            pausedAtTime = plugin.GetTime(videoPlayerId);
            plugin.Pause(videoPlayerId);
            shouldBePlaying = false;
        }

        public bool IsPaused() { return !shouldBePlaying; }

        public void SetVolume(float volume)
        {
            if (isError)
                return;

            plugin.SetVolume(videoPlayerId, volume);
            this.volume = volume;
        }

        public void SetTime(float timeSecs)
        {
            if (isError)
                return;

            pausedAtTime = timeSecs;
            plugin.SetTime(videoPlayerId, timeSecs);
        }

        public void SetLoop(bool loop)
        {
            if (isError)
                return;

            plugin.SetLoop(videoPlayerId, loop);
        }

        public void SetPlaybackRate(float playbackRate)
        {
            if (isError)
                return;

            plugin.SetPlaybackRate(videoPlayerId, playbackRate);
        }

        public float GetTime()
        {
            if (isError)
                return 0;

            return plugin.GetTime(videoPlayerId);
        }

        public float GetDuration()
        {
            if (isError)
                return 0;

            float duration = plugin.GetDuration(videoPlayerId);

            if (float.IsNaN(duration))
                duration = -1;

            return duration;
        }

        public VideoState GetState()
        {
            return (VideoState)plugin.GetState(videoPlayerId);
        }

        public void Dispose()
        {
            plugin.Remove(videoPlayerId);
        }
    }
}