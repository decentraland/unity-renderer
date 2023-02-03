using System;
using System.Collections.Generic;
using DCL.Components.Video.Plugin;
using UnityEngine;

public class VideoPluginWrapper_Native : IVideoPluginWrapper
{
    private Dictionary<string, DCLVideoPlayer> videoPlayers = new Dictionary<string, DCLVideoPlayer>();

    public void Create(string id, string url, bool useHls)
    {
        videoPlayers.Add(id, new DCLVideoPlayer(url));
    }

    public void Remove(string id)
    {
        videoPlayers[id].Dispose();
        videoPlayers.Remove(id);
    }

    public void TextureUpdate(string id)
    {
        videoPlayers[id].UpdateVideoTexture();
    }

    public Texture2D PrepareTexture(string id)
    {
        videoPlayers[id].PrepareTexture();
        return videoPlayers[id].GetTexture();
    }

    public void Play(string id, float startTime)
    {
        videoPlayers[id].Play();

        if (startTime > 0) { videoPlayers[id].SetSeekTime(startTime); }
    }

    public void Pause(string id)
    {
        videoPlayers[id].Pause();
    }

    public void SetVolume(string id, float volume)
    {
        videoPlayers[id].SetVolume(volume);
    }

    public int GetHeight(string id)
    {
        return videoPlayers[id].GetVideoHeight();
    }

    public int GetWidth(string id)
    {
        return videoPlayers[id].GetVideoWidth();
    }

    public float GetTime(string id)
    {
        return videoPlayers[id].GetPlaybackPosition();
    }

    public float GetDuration(string id)
    {
        return videoPlayers[id].GetDuration();
    }

    public VideoState GetState(string id)
    {
        DCLVideoPlayer videoPlayer = videoPlayers[id];

        switch (videoPlayer.GetState())
        {
            case DCLVideoPlayer.VideoPlayerState.Loading:
                return VideoState.LOADING;
            case DCLVideoPlayer.VideoPlayerState.Error:
                return VideoState.ERROR;
            case DCLVideoPlayer.VideoPlayerState.Ready:
                if (videoPlayer.GetTexture() == null) { return VideoState.READY; }

                break;
        }

        if (videoPlayer.IsBuffering()) { return VideoState.BUFFERING; }

        return videoPlayer.IsPlaying() ? VideoState.PLAYING : VideoState.PAUSED;
    }

    public string GetError(string id)
    {
        return "";
    }

    public void SetTime(string id, float second)
    {
        videoPlayers[id].SetSeekTime(second);
    }

    public void SetPlaybackRate(string id, float playbackRate)
    {
        videoPlayers[id].SetPlaybackRate(playbackRate);
    }

    public void SetLoop(string id, bool loop)
    {
        videoPlayers[id].SetLoop(loop);
    }
}
