using System;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public enum VideoType
    {
        Common = 0,
        Hls,
        LiveKit
    }

    public interface IVideoPluginWrapper
    {
        void Create(string id, string url, bool useHls);
        void Create(string id, string url, VideoType videoType);
        void Remove(string id);
        void TextureUpdate(string id);
        Texture2D PrepareTexture(string id);
        void Play(string id, float startTime);
        void Pause(string id);
        void SetVolume(string id, float volume);
        int GetHeight(string id);
        int GetWidth(string id);
        float GetTime(string id);
        float GetDuration(string id);
        VideoState GetState(string id);
        string GetError(string id);
        void SetTime(string id, float second);
        void SetPlaybackRate(string id, float playbackRate);
        void SetLoop(string id, bool loop);
    }
}
