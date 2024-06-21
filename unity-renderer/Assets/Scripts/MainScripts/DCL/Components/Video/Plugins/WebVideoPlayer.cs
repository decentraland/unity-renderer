using System;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayer : IDisposable
    {
        private readonly Vector2Int sceneCoord;
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

        public WebVideoPlayer(string id, string url, bool useHls, IVideoPluginWrapper plugin, Vector2Int sceneCoord)
            : this(id, url, useHls? VideoType.Hls : VideoType.Common, plugin, sceneCoord)
        {
        }

        public WebVideoPlayer(string id, string url, VideoType videoType, IVideoPluginWrapper plugin, Vector2Int sceneCoord)
        {
            videoPlayerId = id;
            this.plugin = plugin;
            this.url = url;
            this.sceneCoord = sceneCoord;
            Debug.Log($"kernel: WebVideoPlayer.ctor: id:{id} url:{url} coord:{sceneCoord} type:{videoType}");
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

                    // Debug.Log($"kernel: WebVideoPlayer.Update.ERROR: url:{url} error:{newError}");

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

                    // Debug.Log($"kernel: WebVideoPlayer.Update.READY: url:{url} playWhenReady:{playWhenReady}");

                    break;
                case VideoState.PLAYING:
                    if (visible)
                        plugin.TextureUpdate(videoPlayerId);

                    // Debug.Log($"kernel: WebVideoPlayer.Update.PLAYING: url:{url} visible:{visible}");

                    break;
            }
        }

        public void Play()
        {
            Debug.Log($"kernel: WebVideoPlayer.Play: url:{url} coord:{sceneCoord} error:{isError} ready:{isReady}");

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
            Debug.Log($"kernel: WebVideoPlayer.PlayInternal: url:{url} coord:{sceneCoord}");
            plugin.Play(videoPlayerId, playStartTime);
            playStartTime = -1;
        }

        public void Pause()
        {
            Debug.Log($"kernel: WebVideoPlayer.Pause: url:{url} coord:{sceneCoord} error: {isError}");

            if (isError)
                return;

            playStartTime = plugin.GetTime(videoPlayerId);
            plugin.Pause(videoPlayerId);
            playWhenReady = false;
        }

        public void SetVolume(float volume)
        {
            Debug.Log($"kernel: WebVideoPlayer.SetVolume: url:{url} coord:{sceneCoord} error: {isError} volume: {volume}");

            if (isError)
                return;

            plugin.SetVolume(videoPlayerId, volume);
            this.volume = volume;
        }

        public void SetTime(float timeSecs)
        {
            Debug.Log($"kernel: WebVideoPlayer.SetTime: url:{url} coord:{sceneCoord} error: {isError} time: {timeSecs}");

            if (isError)
                return;

            playStartTime = timeSecs;
            plugin.SetTime(videoPlayerId, timeSecs);
        }

        public void SetLoop(bool loop)
        {
            Debug.Log($"kernel: WebVideoPlayer.SetLoop: url:{url} coord:{sceneCoord} error: {isError} loop: {loop}");

            if (isError)
                return;

            plugin.SetLoop(videoPlayerId, loop);
        }

        public void SetPlaybackRate(float playbackRate)
        {
            Debug.Log($"kernel: WebVideoPlayer.SetPlaybackRate: url:{url} coord:{sceneCoord} error: {isError} rate: {playbackRate}");

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
            Debug.Log($"kernel: WebVideoPlayer.Dispose: url:{url} coord:{sceneCoord}");
            plugin.Remove(videoPlayerId);
        }
    }
}
