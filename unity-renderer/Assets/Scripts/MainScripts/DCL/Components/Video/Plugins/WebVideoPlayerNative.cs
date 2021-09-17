using System;
using System.Runtime.InteropServices;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayerNative : IWebVideoPlayerPlugin
    {
        public void Create(string id, string url, bool useHls) { WebGLVideoPlugin.WebVideoPlayerCreate(id, url, useHls); }
        public void Remove(string id) { WebGLVideoPlugin.WebVideoPlayerRemove(id); }
        public void TextureUpdate(string id, IntPtr texturePtr, bool isWebGL1) { WebGLVideoPlugin.WebVideoPlayerTextureUpdate(id, texturePtr, isWebGL1); }
        public void Play(string id, float startTime) { WebGLVideoPlugin.WebVideoPlayerPlay(id, startTime); }
        public void Pause(string id) { WebGLVideoPlugin.WebVideoPlayerPause(id); }
        public void SetVolume(string id, float volume) { WebGLVideoPlugin.WebVideoPlayerVolume(id, volume); }
        public int GetHeight(string id) { return WebGLVideoPlugin.WebVideoPlayerGetHeight(id); }
        public int GetWidth(string id) { return WebGLVideoPlugin.WebVideoPlayerGetWidth(id); }
        public float GetTime(string id) { return WebGLVideoPlugin.WebVideoPlayerGetTime(id); }
        public float GetDuration(string id) { return WebGLVideoPlugin.WebVideoPlayerGetDuration(id); }
        public int GetState(string id) { return WebGLVideoPlugin.WebVideoPlayerGetState(id); }
        public string GetError(string id) { return WebGLVideoPlugin.WebVideoPlayerGetError(id); }
        public void SetTime(string id, float second) { WebGLVideoPlugin.WebVideoPlayerSetTime(id, second); }
        public void SetPlaybackRate(string id, float playbackRate) { WebGLVideoPlugin.WebVideoPlayerSetPlaybackRate(id, playbackRate); }
        public void SetLoop(string id, bool loop) { WebGLVideoPlugin.WebVideoPlayerSetLoop(id, loop); }
    }
}