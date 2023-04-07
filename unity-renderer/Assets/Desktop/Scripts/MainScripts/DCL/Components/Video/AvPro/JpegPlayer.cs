using System;
using Cysharp.Threading.Tasks;
using DCL.Components.Video.Plugin;
using UnityEngine;
using UnityEngine.Networking;

public class JpegPlayer : IVideoPlayer, IDisposable
{
    private Texture2D image;
    private VideoState currentState;
    private string lastError;

    public JpegPlayer(string id, string url)
    {
        currentState = VideoState.LOADING;
        DownloadImage(url);
    }

    public async UniTask DownloadImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                lastError = "Error downloading image " + url;
                currentState = VideoState.ERROR;
            }
            else
            {
                image = DownloadHandlerTexture.GetContent(request);
                currentState = VideoState.READY;
            }
        }
    }

    public void Dispose() { GameObject.Destroy(image); }

    public void UpdateVideoTexture(){ }

    public void PrepareTexture() { }

    public Texture2D GetTexture()
    {
        return image;
    }

    public void Play()
    {
        currentState = VideoState.PLAYING;
    }

    public void SetSeekTime(float startTime) { }

    public void Paused()
    {
        currentState = VideoState.PAUSED;

    }

    public void SetVolume(float volume) { }

    public VideoState GetCurrentState()
    {
        return currentState;
    }

    public float GetTime()
    {
        return 0;
    }

    public float GetDuration()
    {
        return 0;
    }

    public void SetPlaybackRate(float playbackRate) { }

    public void SetLoop(bool doLoop) { }

    public string GetLastError()
    {
        return lastError;
    }
}
