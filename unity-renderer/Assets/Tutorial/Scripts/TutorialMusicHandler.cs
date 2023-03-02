using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class TutorialMusicHandler : MonoBehaviour
{
    [SerializeField] AudioEvent tutorialMusic, avatarEditorMusic;

    Coroutine fadeOut;

    private void Awake()
    {
        avatarEditorMusic.OnPlay += OnAvatarEditorMusicPlay;
        avatarEditorMusic.OnStop += OnAvatarEditorMusicStop;
    }

    private void OnDestroy()
    {
        avatarEditorMusic.OnPlay -= OnAvatarEditorMusicPlay;
        avatarEditorMusic.OnStop -= OnAvatarEditorMusicStop;

        if (fadeOut != null)
        {
            CoroutineStarter.Stop(fadeOut);
            fadeOut = null;
        }
    }
    public void StopTutorialMusic()
    {
        DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(1f);
        
        if (fadeOut != null)
        {
            CoroutineStarter.Stop(fadeOut);
            fadeOut = null;
        }
        
        if (tutorialMusic.source.isPlaying)
            fadeOut = CoroutineStarter.Start(tutorialMusic.FadeOut(3f));
    }

    public void TryPlayingMusic()
    {
        if (DCL.Tutorial.TutorialController.i.userAlreadyDidTheTutorial)
            return;

        if (!tutorialMusic.source.isPlaying)
        {
            if (fadeOut != null)
            {
                CoroutineStarter.Stop(fadeOut);
            }
            DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(0f);
            tutorialMusic.Play();
        }
    }

    void OnAvatarEditorMusicPlay()
    {
        if (tutorialMusic.source.isPlaying)
            fadeOut = CoroutineStarter.Start(tutorialMusic.FadeOut(1.5f, false));
    }

    void OnAvatarEditorMusicStop()
    {
        if (tutorialMusic.source.isPlaying)
            CoroutineStarter.Start(tutorialMusic.FadeIn(2.5f));
    }
}