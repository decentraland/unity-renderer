using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components.Video.Plugin;
using RenderHeads.Media.AVProVideo;
using UnityEditor;
using UnityEngine;

public class VideoPluginWrapper_AvPro : IVideoPluginWrapper
{

    private MediaPlayer avProMediaPlayer;
    private VideoState currentState;
    private Texture2D textureReference;
    public void Create(string id, string url, bool useHls)
    {
        avProMediaPlayer = GameObject.Instantiate(Resources.Load<MediaPlayer>("AVProMediaPlayer"), null);
        avProMediaPlayer.name = "AVPRO MEDIA PLAYER FOR " + id;
        avProMediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, url, false);
        avProMediaPlayer.AudioVolume = 0;
        currentState = VideoState.LOADING;
        avProMediaPlayer.Events.AddListener(AvProStateUpdater);
    }
    public void Remove(string id) { GameObject.Destroy(avProMediaPlayer.gameObject); }
    public void TextureUpdate(string id) {  }
    public Texture2D PrepareTexture(string id)
    {
        return (Texture2D)avProMediaPlayer.TextureProducer.GetTexture(0);
    }
    public void Play(string id, float startTime)
    {
        currentState = VideoState.PLAYING;
        avProMediaPlayer.AudioVolume = 1;
        avProMediaPlayer.Play();
    }
    public void Pause(string id)
    {
        currentState = VideoState.PAUSED;
        avProMediaPlayer.Pause();
    }
    public void SetVolume(string id, float volume) { avProMediaPlayer.AudioVolume = volume; }

    public int GetHeight(string id) { throw new System.NotImplementedException(); }
    public int GetWidth(string id) { throw new System.NotImplementedException(); }
    public float GetTime(string id) { return (float)avProMediaPlayer.Control.GetCurrentTime(); }
    public float GetDuration(string id) { return (float)avProMediaPlayer.Info.GetDuration(); }
    public VideoState GetState(string id) { return currentState; }
    public string GetError(string id) { throw new System.NotImplementedException(); }
    public void SetTime(string id, float second) { avProMediaPlayer.Control.SeekFast(second); }
    public void SetPlaybackRate(string id, float playbackRate) { avProMediaPlayer.Control.SetPlaybackRate(playbackRate); }
    public void SetLoop(string id, bool loop) { avProMediaPlayer.Control.SetLooping(loop); }

    private void AvProStateUpdater(MediaPlayer arg0, MediaPlayerEvent.EventType newState, ErrorCode arg2)
    {
        switch (newState)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                currentState = VideoState.READY;
                break;
            case MediaPlayerEvent.EventType.Started:
                currentState = VideoState.PLAYING;
                break;
            case MediaPlayerEvent.EventType.Error:
                currentState = VideoState.ERROR;
                break;
            default:
                break;
        }
    }

}
