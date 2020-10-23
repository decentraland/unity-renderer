using System;
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

    public string imageUrl;
    public int endUnixTimestamp;
    public string title;
    public string body;
    public Button[] buttons;
}


public class WelcomeHUDController : IHUD
{
    internal IWelcomeHUDView view;
    private MessageOfTheDayConfig config = null;

    internal virtual IWelcomeHUDView CreateView() => WelcomeHUDView.CreateView();

    public void Initialize(MessageOfTheDayConfig config)
    {
        this.config = config;
        view = CreateView();
        view.Initialize(OnConfirmPressed, OnClosePressed, config);

        Utils.UnlockCursor();
    }

    internal void Close()
    {
        SetVisibility(false);
        Utils.LockCursor();
    }

    internal virtual void OnConfirmPressed(int buttonIndex)
    {
        Close();

        if (config?.buttons != null && buttonIndex >= 0 && buttonIndex < config?.buttons.Length)
        {
            SendAction(config.buttons[buttonIndex].action);
        }
    }

    void OnClosePressed()
    {
        Close();
    }

    public void Dispose()
    {
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
            Utils.LockCursor();
            AudioScriptableObjects.dialogClose.Play(true);
        }
    }

    internal virtual void SendAction(string action)
    {
        WebInterface.SendChatMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.NONE,
            recipient = string.Empty,
            body = action,
        });
    }
}
