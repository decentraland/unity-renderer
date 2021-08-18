using System;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayerNative : IWebVideoPlayerPlugin
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerCreate(string id, string url, bool useHls);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerRemove(string id);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerTextureUpdate(string id, IntPtr texturePtr, bool isWebGL1);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerPlay(string id, float startTime);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerPause(string id);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerVolume(string id, float volume);
    [DllImport("__Internal")]
    private static extern int WebVideoPlayerGetHeight(string id);
    [DllImport("__Internal")]
    private static extern int WebVideoPlayerGetWidth(string id);
    [DllImport("__Internal")]
    private static extern float WebVideoPlayerGetTime(string id);
    [DllImport("__Internal")]
    private static extern float WebVideoPlayerGetDuration(string id);
    [DllImport("__Internal")]
    private static extern int WebVideoPlayerGetState(string id);
    [DllImport("__Internal")]
    private static extern string WebVideoPlayerGetError(string id);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerSetTime(string id, float second);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerSetPlaybackRate(string id, float playbackRate);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerSetLoop(string id, bool loop);
#else
        private static void WebVideoPlayerCreate(string id, string url, bool useHls) { }
        private static void WebVideoPlayerRemove(string id) { }
        private static void WebVideoPlayerTextureUpdate(string id, IntPtr texturePtr, bool isWebGL1) { }
        private static void WebVideoPlayerPlay(string id, float startTime) { }
        private static void WebVideoPlayerPause(string id) { }
        private static void WebVideoPlayerVolume(string id, float volume) { }
        private static int WebVideoPlayerGetHeight(string id) { return 0; }
        private static int WebVideoPlayerGetWidth(string id) { return 0; }
        private static float WebVideoPlayerGetTime(string id) { return 0; }
        private static float WebVideoPlayerGetDuration(string id) { return 0; }
        private static int WebVideoPlayerGetState(string id) { return (int)VideoState.ERROR; }
        private static string WebVideoPlayerGetError(string id) { return "WebVideoPlayer: Platform not supported"; }
        private static void WebVideoPlayerSetTime(string id, float second) { }
        private static void WebVideoPlayerSetPlaybackRate(string id, float playbackRate) { }
        private static void WebVideoPlayerSetLoop(string id, bool loop) { }
#endif

        public void Create(string id, string url, bool useHls) { WebVideoPlayerCreate(id, url, useHls); }
        public void Remove(string id) { WebVideoPlayerRemove(id); }
        public void TextureUpdate(string id, IntPtr texturePtr, bool isWebGL1) { WebVideoPlayerTextureUpdate(id, texturePtr, isWebGL1); }
        public void Play(string id, float startTime) { WebVideoPlayerPlay(id, startTime); }
        public void Pause(string id) { WebVideoPlayerPause(id); }
        public void SetVolume(string id, float volume) { WebVideoPlayerVolume(id, volume); }
        public int GetHeight(string id) { return WebVideoPlayerGetHeight(id); }
        public int GetWidth(string id) { return WebVideoPlayerGetWidth(id); }
        public float GetTime(string id) { return WebVideoPlayerGetTime(id); }
        public float GetDuration(string id) { return WebVideoPlayerGetDuration(id); }
        public int GetState(string id) { return WebVideoPlayerGetState(id); }
        public string GetError(string id) { return WebVideoPlayerGetError(id); }
        public void SetTime(string id, float second) { WebVideoPlayerSetTime(id, second); }
        public void SetPlaybackRate(string id, float playbackRate) { WebVideoPlayerSetPlaybackRate(id, playbackRate); }
        public void SetLoop(string id, bool loop) { WebVideoPlayerSetLoop(id, loop); }
    }
}