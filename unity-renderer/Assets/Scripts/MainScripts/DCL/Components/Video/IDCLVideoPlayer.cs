using System;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public interface IDCLVideoPlayer : IDisposable
    {
        public event Action<Texture> OnTextureReady;
        public Texture2D texture { get; }
        public float volume { get; }
        public bool playing { get; }
        public bool visible { get; set; }
        public bool isError { get; }
        
        public void UpdateVideoTexture();

        public void Play();

        public void Pause();

        public bool IsPaused();

        public void SetVolume(float volume);

        public void SetTime(float timeSecs);

        public void SetLoop(bool loop);

        public void SetPlaybackRate(float playbackRate);

        public float GetTime();

        public float GetDuration();

        public VideoState GetState();
    }
}