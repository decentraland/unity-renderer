using System;
using System.Runtime.InteropServices;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayerNative : IWebVideoPlayerPlugin
    {
        public void Create(string id, string url, bool useHls) { WebGLPlugin.WebVideoPlayerCreate(id, url, useHls); }
        public void Remove(string id) { WebGLPlugin.WebVideoPlayerRemove(id); }
        public void TextureUpdate(string id, IntPtr texturePtr, bool isWebGL1) { WebGLPlugin.WebVideoPlayerTextureUpdate(id, texturePtr, isWebGL1); }
        public void Play(string id, float startTime) { WebGLPlugin.WebVideoPlayerPlay(id, startTime); }
        public void Pause(string id) { WebGLPlugin.WebVideoPlayerPause(id); }
        public void SetVolume(string id, float volume) { WebGLPlugin.WebVideoPlayerVolume(id, volume); }
        public int GetHeight(string id) { return WebGLPlugin.WebVideoPlayerGetHeight(id); }
        public int GetWidth(string id) { return WebGLPlugin.WebVideoPlayerGetWidth(id); }
        public float GetTime(string id) { return WebGLPlugin.WebVideoPlayerGetTime(id); }
        public float GetDuration(string id) { return WebGLPlugin.WebVideoPlayerGetDuration(id); }
        public int GetState(string id) { return WebGLPlugin.WebVideoPlayerGetState(id); }
        public string GetError(string id) { return WebGLPlugin.WebVideoPlayerGetError(id); }
        public void SetTime(string id, float second) { WebGLPlugin.WebVideoPlayerSetTime(id, second); }
        public void SetPlaybackRate(string id, float playbackRate) { WebGLPlugin.WebVideoPlayerSetPlaybackRate(id, playbackRate); }
        public void SetLoop(string id, bool loop) { WebGLPlugin.WebVideoPlayerSetLoop(id, loop); }
    }
}