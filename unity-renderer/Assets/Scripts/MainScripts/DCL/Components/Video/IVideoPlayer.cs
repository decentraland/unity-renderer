using System;
using DCL.Components.Video.Plugin;
using UnityEngine;

namespace DCL.Components.Video
{
    public interface IVideoPlayer : IDisposable
    {
        Texture2D texture { get; }
        float volume { get; }
        bool playing { get; }
        bool visible { get; set; }
        bool isError { get; }
        void UpdateWebVideoTexture();
        void Play();
        void Pause();
        bool IsPaused();
        void SetVolume(float volume);
        void SetTime(float timeSecs);
        void SetLoop(bool loop);
        void SetPlaybackRate(float playbackRate);
        float GetTime();
        float GetDuration();
        VideoState GetState();
        Texture2D CreateTexture(int width, int height);
    }
}