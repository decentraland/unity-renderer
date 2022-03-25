using System;
using DCL.Interface;
using UnityEngine;

public interface IPrivateChatComponentView
{
    event Action OnPressBack;
    event Action<ChatMessage> OnSendMessage;
    event Action OnMinimize;
    event Action OnClose;
    
    IChatHUDComponentView ChatHUD { get; }
    bool IsActive { get; }
    RectTransform Transform { get; }

    void Setup(UserProfile profile, bool isOnline, bool isBlocked);
    void Show();
    void Hide();
    void Dispose();
}