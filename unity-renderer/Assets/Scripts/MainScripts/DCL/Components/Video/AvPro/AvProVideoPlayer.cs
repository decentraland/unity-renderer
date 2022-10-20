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
    private bool resizeDone;
    private bool doLoop;

    private CancellationTokenSource resizeVideoTextureCancellationSource;
    
    
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
                resizeDone = false;
                avProTexture = null;
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
        resizeVideoTextureCancellationSource?.Cancel();
        resizeVideoTextureCancellationSource?.Dispose();
        resizeVideoTextureCancellationSource = null;
        GameObject.Destroy(avProMediaPlayer.gameObject);
    }

    public void UpdateVideoTexture()
    {
        if (avProTexture && (avProTexture.width != videoTexture.width ||
                             avProTexture.height != videoTexture.height))
        {
            StartResizeTexture();
            return;
        }
        if (resizeDone)
        {
            avProTexture = avProMediaPlayer.TextureProducer.GetTexture(0);
            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
            {
                //High GC allocs here
                videoTexture.SetPixels(((Texture2D)avProTexture).GetPixels()));
                videoTexture.Apply();
            } else {
                Graphics.CopyTexture(avProTexture, videoTexture);
            }
        
            
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary( 
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);


// Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);


// Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;


// Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;


// Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);


// Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();


// Reset the active RenderTexture
            RenderTexture.active = previous;


// Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);


// "myTexture2D" now has the same pixels from "texture" and it's re
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
        videoTexture.Resize(avProTexture.width, avProTexture.height);
        videoTexture.Apply();
        resizeDone = true;
    }

    private void StartResizeTexture()
    {
        resizeDone = false;
        resizeVideoTextureCancellationSource?.Cancel();
        resizeVideoTextureCancellationSource?.Dispose();
        resizeVideoTextureCancellationSource = new CancellationTokenSource();
        ResizeVideoTexture(resizeVideoTextureCancellationSource.Token);
    }

    public string GetLastError()
    {
        return lastError;
    }
}
