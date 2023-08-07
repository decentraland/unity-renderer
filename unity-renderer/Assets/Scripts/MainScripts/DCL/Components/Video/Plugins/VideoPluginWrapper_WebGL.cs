using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DCL.Components.Video.Plugin
{
    public class VideoPluginWrapper_WebGL : IVideoPluginWrapper
    {
        public void Create(string id, string url, bool useHls) { Create(id, url, useHls? VideoType.Hls : VideoType.Common); }
        public void Create(string id, string url, VideoType videoType) { WebGLVideoPlugin.WebVideoPlayerCreate(id, url, (int)videoType); }
        public void Remove(string id) { WebGLVideoPlugin.WebVideoPlayerRemove(id); }
        public void TextureUpdate(string id) { WebGLVideoPlugin.WebVideoPlayerTextureUpdate(id); }

        public Texture2D PrepareTexture(string id)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.UpdateExternalTexture((IntPtr)WebGLVideoPlugin.WebVideoPlayerTextureGet(id));
            texture.Apply();
            return texture;
        }

        public void Play(string id, float startTime) { WebGLVideoPlugin.WebVideoPlayerPlay(id, startTime); }
        public void Pause(string id) { WebGLVideoPlugin.WebVideoPlayerPause(id); }
        public void SetVolume(string id, float volume) { WebGLVideoPlugin.WebVideoPlayerVolume(id, volume); }
        public int GetHeight(string id) { return WebGLVideoPlugin.WebVideoPlayerGetHeight(id); }
        public int GetWidth(string id) { return WebGLVideoPlugin.WebVideoPlayerGetWidth(id); }
        public float GetTime(string id) { return WebGLVideoPlugin.WebVideoPlayerGetTime(id); }
        public float GetDuration(string id) { return WebGLVideoPlugin.WebVideoPlayerGetDuration(id); }
        public VideoState GetState(string id) { return (VideoState)WebGLVideoPlugin.WebVideoPlayerGetState(id); }
        public string GetError(string id) { return WebGLVideoPlugin.WebVideoPlayerGetError(id); }
        public void SetTime(string id, float second) { WebGLVideoPlugin.WebVideoPlayerSetTime(id, second); }
        public void SetPlaybackRate(string id, float playbackRate) { WebGLVideoPlugin.WebVideoPlayerSetPlaybackRate(id, playbackRate); }
        public void SetLoop(string id, bool loop) { WebGLVideoPlugin.WebVideoPlayerSetLoop(id, loop); }
    }
}
