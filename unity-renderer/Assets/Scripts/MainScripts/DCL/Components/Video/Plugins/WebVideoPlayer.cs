using System;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayer : IDisposable
    {
        private const float DEFAULT_ASPECT_RATIO = 16.0f / 9.0f;
        
        public readonly string url;

        private string videoPlayerId;

        private readonly IVideoPluginWrapper plugin;

        private bool playWhenReady = false;
        private float playStartTime = -1;

        private string lastError = "";

        
        public Texture2D Texture { private set; get; }
        public float Volume { private set; get; }
        public bool Playing => GetState() == VideoState.PLAYING;
        public bool IsError => GetState() == VideoState.ERROR;
        public bool Visible { get; set; } = true;
        public bool Loading { get; private set; } = false;
        public bool IsReady { get; private set; } = false;
        

        public WebVideoPlayer(string id, string url, bool useHls, IVideoPluginWrapper plugin)
        {
            videoPlayerId = id;
            this.plugin = plugin;
            this.url = url;
            plugin.Create(id, url, useHls);
        }

        public void Update()
        {
            switch (plugin.GetState(videoPlayerId))
            {
                case VideoState.ERROR:
                    string newError = plugin.GetError(videoPlayerId);
                    if (newError == lastError)
                        break;

                    UpdateTextureConservingAspectRatio(Resources.Load<Texture2D>("Textures/VideoFailed"), true);
                    lastError = newError;
                    Debug.LogError(lastError);

                    Loading = false;
                    break;
                case VideoState.LOADING:
                    if (Loading)
                        break;

                    UpdateTextureConservingAspectRatio(Resources.Load<Texture2D>("Textures/VideoLoading"), true);
                    Loading = true;
                    break;
                case VideoState.READY:
                    if (!IsReady)
                    {
                        IsReady = true;
                        Texture = plugin.PrepareTexture(videoPlayerId);
                    }

                    if (playWhenReady)
                    {
                        PlayInternal();
                        playWhenReady = false;
                    }

                    Loading = false;
                    break;
                case VideoState.PLAYING:
                    if (Visible)
                        plugin.TextureUpdate(videoPlayerId);
                    break;
            }
        }

        public void Play()
        {
            if (IsError)
                return;

            if (!IsReady)
            {
                playWhenReady = true;
                return;
            }

            PlayInternal();
        }

        private void PlayInternal()
        {
            plugin.Play(videoPlayerId, playStartTime);
            playStartTime = -1;
        }

        public void Pause()
        {
            if (IsError)
                return;

            playStartTime = plugin.GetTime(videoPlayerId);
            plugin.Pause(videoPlayerId);
            playWhenReady = false;
        }

        public void SetVolume(float volume)
        {
            if (IsError)
                return;

            plugin.SetVolume(videoPlayerId, volume);
            this.Volume = volume;
        }

        public void SetTime(float timeSecs)
        {
            if (IsError)
                return;

            playStartTime = timeSecs;
            plugin.SetTime(videoPlayerId, timeSecs);
        }

        public void SetLoop(bool loop)
        {
            if (IsError)
                return;

            plugin.SetLoop(videoPlayerId, loop);
        }

        public void SetPlaybackRate(float playbackRate)
        {
            if (IsError)
                return;

            plugin.SetPlaybackRate(videoPlayerId, playbackRate);
        }

        public float GetTime()
        {
            if (IsError)
                return 0;

            return plugin.GetTime(videoPlayerId);
        }

        public float GetDuration()
        {
            if (IsError)
                return 0;

            float duration = plugin.GetDuration(videoPlayerId);

            if (float.IsNaN(duration))
                duration = -1;

            return duration;
        }

        public VideoState GetState()
        {
            return plugin.GetState(videoPlayerId);
        }

        public void Dispose()
        {
            plugin.Remove(videoPlayerId);
        }


        private void UpdateTextureConservingAspectRatio(Texture2D textureToCenter, bool fillBackground)
        {
            // Avoiding Memory leaks as textures are never destroyed by Unity
            if (Texture != null)
            {
                UnityEngine.Object.Destroy(Texture);
                Texture = null;
            }

            // Fix textureToCenter in the center of a 16:9 texture
            int xOffset = 0;
            int yOffset = 0;
            if ((float)textureToCenter.width / textureToCenter.height >= DEFAULT_ASPECT_RATIO)
            {
                Texture = new Texture2D(textureToCenter.width, Mathf.RoundToInt(textureToCenter.width / DEFAULT_ASPECT_RATIO));
                yOffset = (Texture.height - textureToCenter.height) / 2;
            }
            else
            {
                Texture = new Texture2D(Mathf.RoundToInt(textureToCenter.height * DEFAULT_ASPECT_RATIO), textureToCenter.height);
                xOffset = (Texture.width - textureToCenter.width) / 2;
            }

            if (fillBackground)
            {
                Color32[] backgroundPixels = new Color32[Texture.width * Texture.height];
                Color32 color = textureToCenter.GetPixel(0, 0);
                Array.Fill(backgroundPixels, color);
                Texture.SetPixels32(backgroundPixels);
            }

            Color32[] pixels = textureToCenter.GetPixels32(0);
            Texture.SetPixels32(xOffset, yOffset, textureToCenter.width, textureToCenter.height, pixels);
            Texture.Apply();
        }
    }
}
