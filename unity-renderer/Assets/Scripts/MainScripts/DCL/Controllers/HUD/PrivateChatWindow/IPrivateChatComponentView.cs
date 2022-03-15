using System;
using DCL.Interface;
using UnityEngine;

public interface IPrivateChatComponentView
{
    event Action OnPressBack;
    event Action OnInputFieldSelected;
    event Action<ChatMessage> OnSendMessage;
    event Action OnMinimize;
    event Action OnClose;
    
    IChatHUDComponentView ChatHUD { get; }
    bool IsActive { get; }
    RectTransform Transform { get; }

    void Setup(UserProfile profile);
    void CleanAllEntries();
    void ResetInputField();
    void FocusInputField();
    void Show();
    void Hide();
    void Dispose();
}