using System;
using DCL.Interface;
using UnityEngine;

public interface IChannelChatWindowView
{
    event Action OnClose;
    event Action<string> OnMessageUpdated;
    event Action<ChatMessage> OnSendMessage;
    event Action OnDeactivatePreview;
    event Action OnActivatePreview;
    
    bool IsActive { get; }
    bool IsPreview { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }

    void ActivatePreview();
    void DeactivatePreview();
    void Dispose();
    void Hide();
    void Show();
    void Setup(string channelId, string name, string description);
}