using System;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayer : IDisposable
    {
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
            : this(id, url, useHls? VideoType.Hls : VideoType.Common, plugin)
        {
        }

        public WebVideoPlayer(string id, string url, VideoType videoType, IVideoPluginWrapper plugin)
        {
            videoPlayerId = id;
            this.plugin = plugin;
            this.url = url;
            plugin.Create(id, url, videoType);
        }

        public void Update()
        {
            switch (plugin.GetState(videoPlayerId))
            {
                case VideoState.ERROR:
                    string newError = plugin.GetError(videoPlayerId);

                    if ( newError != lastError )
                    {
                        lastError = newError;
                        Debug.LogError(lastError);
                    }

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
            return (VideoState)plugin.GetState(videoPlayerId);
        }

        public void Dispose()
        {
            plugin.Remove(videoPlayerId);
        }
    }
}
