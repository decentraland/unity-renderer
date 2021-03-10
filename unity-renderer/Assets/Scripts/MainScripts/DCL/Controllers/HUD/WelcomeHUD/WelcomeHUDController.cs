using System;
using System.Collections;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

/// <summary>
/// Model with the configuration for a Message of the Day
/// </summary>
[Serializable]
public class MessageOfTheDayConfig
{
    /// <summary>
    /// Model with the configuration for each button in the Message of the Day
    /// </summary>
    [Serializable]
    public class Button
    {
        public string caption;
        public string action; //global chat action to perform
        public Color tint;
    }

    public string background_banner;
    public int endUnixTimestamp;
    public string title;
    public string body;
    public Button[] buttons;
}


public class WelcomeHUDController : IHUD
{
    const float POPUP_DELAY = 2;

    internal IWelcomeHUDView view;
    private MessageOfTheDayConfig config = null;
    private Coroutine showPopupDelayedRoutine;
    private bool isPopupRoutineRunning = false;

    internal virtual IWelcomeHUDView CreateView() => WelcomeHUDView.CreateView();

    public WelcomeHUDController()
    {
        view = CreateView();
        view.SetVisible(false);

        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActive_OnChange;
        CommonScriptableObjects.tutorialActive.OnChange += TutorialActive_OnChange;

        CommonScriptableObjects.emailPromptActive.OnChange -= EmailPromptActive_OnChange;
        CommonScriptableObjects.emailPromptActive.OnChange += EmailPromptActive_OnChange;
    }

    public void Initialize(MessageOfTheDayConfig config)
    {
        this.config = config;

        if (!isPopupRoutineRunning)
            StartPopupRoutine();
        else
            ResetPopupDelayed();
    }

    internal void Close()
    {
        SetVisibility(false);
    }

    internal virtual void OnConfirmPressed(int buttonIndex)
    {
        Close();

        if (config?.buttons != null && buttonIndex >= 0 && buttonIndex < config?.buttons.Length)
        {
            string action = config.buttons[buttonIndex].action;
            SendAction(action);
        }
    }

    void OnClosePressed()
    {
        Close();
    }

    public void Dispose()
    {
        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActive_OnChange;
        CommonScriptableObjects.emailPromptActive.OnChange -= EmailPromptActive_OnChange;

        view?.DisposeSelf();
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisible(visible);
        if (visible)
        {
            Utils.UnlockCursor();
            AudioScriptableObjects.dialogOpen.Play(true);
        }
        else
        {
            AudioScriptableObjects.dialogClose.Play(true);
        }

        CommonScriptableObjects.motdActive.Set(visible);
    }

    internal virtual void SendAction(string action)
    {
        if (string.IsNullOrEmpty(action))
            return;

        WebInterface.SendChatMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.NONE,
            recipient = string.Empty,
            body = action,
        });
    }

    void StartPopupRoutine()
    {
        showPopupDelayedRoutine = CoroutineStarter.Start(ShowPopupDelayed(POPUP_DELAY));
    }

    void StopPopupRoutine()
    {
        if (showPopupDelayedRoutine != null)
        {
            CoroutineStarter.Stop(showPopupDelayedRoutine);
            showPopupDelayedRoutine = null;
        }
        isPopupRoutineRunning = false;
    }

    private IEnumerator ShowPopupDelayed(float seconds)
    {
        isPopupRoutineRunning = true;
        view.Initialize(OnConfirmPressed, OnClosePressed, config);

        yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());
        yield return new WaitUntil(() => !CommonScriptableObjects.tutorialActive.Get());
        yield return new WaitUntil(() => !CommonScriptableObjects.emailPromptActive.Get());
        yield return WaitForSecondsCache.Get(seconds);
        yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

        SetVisibility(true);
        isPopupRoutineRunning = false;
    }

    void ResetPopupDelayed()
    {
        if (isPopupRoutineRunning)
        {
            StopPopupRoutine();
            StartPopupRoutine();
        }
    }

    private void TutorialActive_OnChange(bool current, bool previous)
    {
        if (current)
            ResetPopupDelayed();
    }

    private void EmailPromptActive_OnChange(bool current, bool previous)
    {
        if (current)
            ResetPopupDelayed();
    }
}