using System;
using UnityEngine;

public interface IPrivateChatComponentView
{
    event Action OnPressBack;
    event Action OnMinimize;
    event Action OnClose;
    event Action<string> OnUnfriend;
    
    IChatHUDComponentView ChatHUD { get; }
    bool IsActive { get; }
    RectTransform Transform { get; }

    void Setup(UserProfile profile, bool isOnline, bool isBlocked);
    void Show();
    void Hide();
    void Dispose();
}