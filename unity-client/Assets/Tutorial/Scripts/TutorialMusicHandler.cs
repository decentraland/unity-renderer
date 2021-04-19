using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMusicHandler : MonoBehaviour
{
    [SerializeField] DCL.Tutorial.TutorialController tutorialController;
    [SerializeField] AudioEvent tutorialMusic, avatarEditorMusic;

    bool rendererIsReady = false, tutorialHasBeenEnabled = false;

    Coroutine fadeOut;

    private void Awake()
    {
        CommonScriptableObjects.tutorialActive.OnChange += TutorialActive_OnChange;
        CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        avatarEditorMusic.OnPlay += OnAvatarEditorMusicPlay;
        avatarEditorMusic.OnStop += OnAvatarEditorMusicStop;
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActive_OnChange;
        CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChange;
        avatarEditorMusic.OnPlay -= OnAvatarEditorMusicPlay;
        avatarEditorMusic.OnStop -= OnAvatarEditorMusicStop;
    }

    void OnRendererStateChange(bool current, bool previous)
    {
        rendererIsReady = current;
        TryPlayingMusic();
    }

    private void TutorialActive_OnChange(bool current, bool previous)
    {
        if (current)
        {
            tutorialHasBeenEnabled = true;
            TryPlayingMusic();
        }
        else
        {
            if (tutorialMusic.source.isPlaying)
                fadeOut = CoroutineStarter.Start(tutorialMusic.FadeOut(3f));
            tutorialHasBeenEnabled = false;
        }
    }

    void TryPlayingMusic()
    {
        if (tutorialController.userAlreadyDidTheTutorial)
            return;

        if (rendererIsReady && tutorialHasBeenEnabled && !tutorialMusic.source.isPlaying)
        {
            if (fadeOut != null)
            {
                CoroutineStarter.Stop(fadeOut);
            }
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
