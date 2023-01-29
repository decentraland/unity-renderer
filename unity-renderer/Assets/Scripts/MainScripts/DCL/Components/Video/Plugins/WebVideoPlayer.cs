using System;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayer : IDisposable
    {
        private const float DEFAULT_ASPECT_RATIO = 16.0f / 9.0f;
        
        public Texture2D texture { private set; get; }
        public float volume { private set; get; }
        public bool playing => GetState() == VideoState.PLAYING;
        public bool isError => GetState() == VideoState.ERROR;
        public bool visible { get; set; } = true;
        public bool isReady { get; private set; } = false;

        public readonly string url;

        private string videoPlayerId;

        private readonly IVideoPluginWrapper plugin;

        private bool playWhenReady = false;
        private float playStartTime = -1;

        private string lastError = "";

        public WebVideoPlayer(string id, string url, bool useHls, IVideoPluginWrapper plugin)
        {
            videoPlayerId = id;
            this.plugin = plugin;
            this.url = url;
            plugin.Create(id, url, useHls);

            UpdateTextureConservingAspectRatio(Resources.Load<Texture2D>("Textures/VideoLoading"), true);
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

                    break;
                case VideoState.READY:
                    if (!isReady)
                    {
                        isReady = true;
                        texture = plugin.PrepareTexture(videoPlayerId);
                    }

                    if (playWhenReady)
                    {
                        PlayInternal();
                        playWhenReady = false;
                    }

                    break;
                case VideoState.PLAYING:
                    if (visible)
                        plugin.TextureUpdate(videoPlayerId);

                    break;
            }
        }

        public void Play()
        {
            if (isError)
                return;

            if (!isReady)
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
            if (isError)
                return;

            playStartTime = plugin.GetTime(videoPlayerId);
            plugin.Pause(videoPlayerId);
            playWhenReady = false;
        }

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

            playStartTime = timeSecs;
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
            return plugin.GetState(videoPlayerId);
        }

        public void SetAsError()
        {
            UpdateTextureConservingAspectRatio(Resources.Load<Texture2D>("Textures/VideoFailed"), true);
        }

        public void Dispose()
        {
            plugin.Remove(videoPlayerId);
        }


        private void UpdateTextureConservingAspectRatio(Texture2D textureToCenter, bool fillBackground)
        {
            // Avoiding Memory leaks as textures are never destroyed by Unity
            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
                texture = null;
            }

            // Fix textureToCenter in the center of a 16:9 texture
            int xOffset = 0;
            int yOffset = 0;
            if ((float)textureToCenter.width / textureToCenter.height >= DEFAULT_ASPECT_RATIO)
            {
                texture = new Texture2D(textureToCenter.width, Mathf.RoundToInt(textureToCenter.width / DEFAULT_ASPECT_RATIO));
                yOffset = (texture.height - textureToCenter.height) / 2;
            }
            else
            {
                texture = new Texture2D(Mathf.RoundToInt(textureToCenter.height * DEFAULT_ASPECT_RATIO), textureToCenter.height);
                xOffset = (texture.width - textureToCenter.width) / 2;
            }

            if (fillBackground)
            {
                Color32[] backgroundPixels = new Color32[texture.width * texture.height];
                Color32 color = textureToCenter.GetPixel(0, 0);
                Array.Fill(backgroundPixels, color);
                texture.SetPixels32(backgroundPixels);
            }

            Color32[] pixels = textureToCenter.GetPixels32(0);
            texture.SetPixels32(xOffset, yOffset, textureToCenter.width, textureToCenter.height, pixels);
            texture.Apply();
        }
    }
}
