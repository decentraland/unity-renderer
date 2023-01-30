using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Components.Video.Plugin;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

public class AvProVideoPlayer : IVideoPlayer, IDisposable
{
    private MediaPlayer avProMediaPlayer;
    private VideoState currentState;
    private Texture2D videoTexture;
    private Texture avProTexture;
    private string lastError;
    private string id;
    private bool doLoop;

    private CancellationTokenSource resizeVideoTextureCts;
    
    
    public AvProVideoPlayer(string id, string url)
    {
        avProMediaPlayer = GameObject.Instantiate(Resources.Load<MediaPlayer>("AVPro/AVProMediaPlayer"), null);
        this.id = id;
        avProMediaPlayer.name = "_AvProMediaFor_" + id;
        avProMediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, url, false);
        avProMediaPlayer.AudioVolume = 0;
        currentState = VideoState.LOADING;
        avProMediaPlayer.Events.AddListener(AvProStateUpdater);
    }

    private void AvProStateUpdater(MediaPlayer arg0, MediaPlayerEvent.EventType newState, ErrorCode arg2)
    {
        switch (newState)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                avProTexture = avProMediaPlayer.TextureProducer.GetTexture(0);
                currentState = VideoState.READY;
                break;
            case MediaPlayerEvent.EventType.Started:
                currentState = VideoState.PLAYING;
                break;
            case MediaPlayerEvent.EventType.Error:
                lastError = $"AVProError {arg2} for id {id}";
                avProMediaPlayer.CloseMedia();
                currentState = VideoState.ERROR;
                break;
            case MediaPlayerEvent.EventType.ResolutionChanged:
                StartResizeTexture();
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                if (doLoop)
                {
                    avProMediaPlayer.Rewind(false);
                    avProMediaPlayer.Play();
                }
                break;
        }
    }

    public void Dispose()
    {
        avProMediaPlayer.Events.RemoveListener(AvProStateUpdater);
        avProMediaPlayer.CloseMedia();
        resizeVideoTextureCts?.Cancel();
        resizeVideoTextureCts?.Dispose();
        resizeVideoTextureCts = null;
        GameObject.Destroy(avProMediaPlayer.gameObject);
    }

    public void UpdateVideoTexture()
    {
        if (avProTexture && HasEqualResolution())
        {
            avProTexture = avProMediaPlayer.TextureProducer.GetTexture(0);
            Graphics.CopyTexture(avProTexture, videoTexture);
        }
        else if (avProTexture && !HasEqualResolution())
        {
            StartResizeTexture();
        }
    }

    public void PrepareTexture()
    {
        //Required BGRA32 for compatibility with AVPro texture
        videoTexture = new Texture2D(1, 1, TextureFormat.BGRA32, false, false);
    }

    public Texture2D GetTexture()
    {
        return videoTexture;
    }

    public void Play()
    {
        currentState = VideoState.PLAYING;
        avProMediaPlayer.Play();
    }

    public void SetSeekTime(float startTime)
    {
        avProMediaPlayer.Control.SeekFast(startTime);
    }

    public void Paused()
    {
        currentState = VideoState.PAUSED;
        avProMediaPlayer.Pause();
    }

    public void SetVolume(float volume)
    {
        avProMediaPlayer.AudioVolume = volume;
    }

    public VideoState GetCurrentState()
    {
        return currentState;
    }

    public float GetTime()
    {
        return (float)avProMediaPlayer.Control.GetCurrentTime();
    }

    public float GetDuration()
    {
        return (float)avProMediaPlayer.Info.GetDuration();
    }

    public void SetPlaybackRate(float playbackRate)
    {
        avProMediaPlayer.PlaybackRate = playbackRate;
    }

    public void SetLoop(bool doLoop)
    {
        this.doLoop = doLoop;
    }

    public async UniTask ResizeVideoTexture(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        while (videoTexture == null || avProTexture == null)
        {
            avProTexture = avProMediaPlayer.TextureProducer.GetTexture(0);
            await UniTask.Yield(ct).AttachExternalCancellation(ct);
        }
        videoTexture.Reinitialize(avProTexture.width, avProTexture.height);
        videoTexture.Apply();
    }

    private void StartResizeTexture()
    {
        avProTexture = null;
        resizeVideoTextureCts?.Cancel();
        resizeVideoTextureCts?.Dispose();
        resizeVideoTextureCts = new CancellationTokenSource();
        ResizeVideoTexture(resizeVideoTextureCts.Token);
    }

    public string GetLastError()
    {
        return lastError;
    }
    
    private bool HasEqualResolution()
    {
        return avProTexture.width == videoTexture.width && avProTexture.height == videoTexture.height;
    }


}
