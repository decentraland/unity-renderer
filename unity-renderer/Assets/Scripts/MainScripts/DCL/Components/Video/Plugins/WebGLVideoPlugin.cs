using System;
using System.Runtime.InteropServices;

namespace DCL.Components.Video.Plugin
{
    public static class WebGLVideoPlugin
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerCreate(string id, string url, int videoType);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerRemove(string id);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerTextureUpdate(string id);
        [DllImport("__Internal")]
        public static extern int WebVideoPlayerTextureGet(string id);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerPlay(string id, float startTime);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerPause(string id);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerVolume(string id, float volume);
        [DllImport("__Internal")]
        public static extern int WebVideoPlayerGetHeight(string id);
        [DllImport("__Internal")]
        public static extern int WebVideoPlayerGetWidth(string id);
        [DllImport("__Internal")]
        public static extern float WebVideoPlayerGetTime(string id);
        [DllImport("__Internal")]
        public static extern float WebVideoPlayerGetDuration(string id);
        [DllImport("__Internal")]
        public static extern int WebVideoPlayerGetState(string id);
        [DllImport("__Internal")]
        public static extern string WebVideoPlayerGetError(string id);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerSetTime(string id, float second);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerSetPlaybackRate(string id, float playbackRate);
        [DllImport("__Internal")]
        public static extern void WebVideoPlayerSetLoop(string id, bool loop);
#else
        public static void WebVideoPlayerCreate(string id, string url, int videoType) { }
        public static void WebVideoPlayerRemove(string id) { }
        public static void WebVideoPlayerTextureUpdate(string id) { }
        public static int WebVideoPlayerTextureGet(string id) { return -1; }
        public static void WebVideoPlayerPlay(string id, float startTime) { }
        public static void WebVideoPlayerPause(string id) { }
        public static void WebVideoPlayerVolume(string id, float volume) { }
        public static int WebVideoPlayerGetHeight(string id) { return 0; }
        public static int WebVideoPlayerGetWidth(string id) { return 0; }
        public static float WebVideoPlayerGetTime(string id) { return 0; }
        public static float WebVideoPlayerGetDuration(string id) { return 0; }
        public static int WebVideoPlayerGetState(string id) { return (int)VideoState.ERROR; }
        public static string WebVideoPlayerGetError(string id) { return "WebVideoPlayer: Platform not supported"; }
        public static void WebVideoPlayerSetTime(string id, float second) { }
        public static void WebVideoPlayerSetPlaybackRate(string id, float playbackRate) { }
        public static void WebVideoPlayerSetLoop(string id, bool loop) { }
#endif
    }
}
