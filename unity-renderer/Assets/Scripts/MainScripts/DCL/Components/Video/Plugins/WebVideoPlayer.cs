using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace DCL.Components.Video.Plugin
{
    public class WebVideoPlayer : IDisposable
    {
        public event Action<Texture> OnTextureReady;
        public Texture2D texture { private set; get; }
        public float volume { private set; get; }
        public bool playing { get { return shouldBePlaying; } }
        public bool visible { get; set; }
        public bool isError { get; private set; }

        private enum VideoState { NONE = 0, ERROR = 1, LOADING = 2, READY = 3, PLAYING = 4, BUFFERING = 5 };

        private static bool isWebGL1 => SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2;

        private string videoPlayerId;
        private IntPtr textureNativePtr;
        private bool initialized = false;
        private bool shouldBePlaying = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerCreate(string id, string url, bool useHls);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerRemove(string id);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerTextureUpdate(string id, IntPtr texturePtr, bool isWebGL1);
    [DllImport("__Internal")]
    private static extern void WebVideoPlayerPlay(string id);
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
        private static void WebVideoPlayerPlay(string id) { }
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

        public WebVideoPlayer(string id, string url, bool useHls)
        {
            videoPlayerId = id;

            WebVideoPlayerCreate(id, url, useHls);
        }

        public void UpdateWebVideoTexture()
        {
            if (isError)
            {
                return;
            }

            switch (WebVideoPlayerGetState(videoPlayerId))
            {
                case (int)VideoState.ERROR:
                    Debug.LogError(WebVideoPlayerGetError(videoPlayerId));
                    isError = true;
                    break;
                case (int)VideoState.READY:
                    if (!initialized)
                    {
                        initialized = true;
                        texture = CreateTexture(WebVideoPlayerGetWidth(videoPlayerId), WebVideoPlayerGetHeight(videoPlayerId));
                        textureNativePtr = texture.GetNativeTexturePtr();
                        OnTextureReady?.Invoke(texture);
                    }
                    break;
                case (int)VideoState.PLAYING:
                    if (shouldBePlaying && visible)
                    {
                        int width = WebVideoPlayerGetWidth(videoPlayerId);
                        int height = WebVideoPlayerGetHeight(videoPlayerId);
                        if (texture.width != width || texture.height != height)
                        {
                            if (texture.Resize(width, height))
                            {
                                texture.Apply();
                                textureNativePtr = texture.GetNativeTexturePtr();
                            }
                        }
                        if (texture.width > 0 && texture.height > 0)
                        {
                            WebVideoPlayerTextureUpdate(videoPlayerId, textureNativePtr, isWebGL1);
                        }
                    }
                    break;
            }
        }

        public void Play()
        {
            if (isError)
                return;

            WebVideoPlayerPlay(videoPlayerId);
            shouldBePlaying = true;
        }

        public void Pause()
        {
            if (isError)
                return;

            WebVideoPlayerPause(videoPlayerId);
            shouldBePlaying = false;
        }

        public bool IsPaused()
        {
            return !shouldBePlaying;
        }

        public void SetVolume(float volume)
        {
            if (isError)
                return;

            WebVideoPlayerVolume(videoPlayerId, volume);
            this.volume = volume;
        }

        public void SetTime(float timeSecs)
        {
            if (isError)
                return;

            WebVideoPlayerSetTime(videoPlayerId, timeSecs);
        }

        public void SetLoop(bool loop)
        {
            if (isError)
                return;

            WebVideoPlayerSetLoop(videoPlayerId, loop);
        }

        public void SetPlaybackRate(float playbackRate)
        {
            if (isError)
                return;

            WebVideoPlayerSetPlaybackRate(videoPlayerId, playbackRate);
        }

        public float GetTime()
        {
            if (isError)
                return 0;

            return WebVideoPlayerGetTime(videoPlayerId);
        }

        public float GetDuration()
        {
            if (isError)
                return 0;

            return WebVideoPlayerGetDuration(videoPlayerId);
        }

        public void Dispose()
        {
            WebVideoPlayerRemove(videoPlayerId);
            UnityEngine.Object.Destroy(texture);
            texture = null;
        }

        private Texture2D CreateTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }
    }
}