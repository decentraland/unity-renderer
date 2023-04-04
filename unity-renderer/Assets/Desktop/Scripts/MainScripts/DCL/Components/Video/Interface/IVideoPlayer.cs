using DCL.Components.Video.Plugin;
using UnityEngine;

public interface IVideoPlayer
{
    public void UpdateVideoTexture();

    public void PrepareTexture();

    public Texture2D GetTexture();

    public void Play();

    public void SetSeekTime(float startTime);

    public void Paused();

    public void SetVolume(float volume);

    public VideoState GetCurrentState();

    public float GetTime();

    public float GetDuration();

    public void SetPlaybackRate(float playbackRate);

    public void SetLoop(bool doLoop);

    public string GetLastError();

    public void Dispose();
}
