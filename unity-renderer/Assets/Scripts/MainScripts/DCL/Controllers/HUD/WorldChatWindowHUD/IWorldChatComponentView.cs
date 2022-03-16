using System;
using DCL.Interface;
using UnityEngine;

public interface IWorldChatComponentView
{
    event Action OnClose;
    event Action<string> OnMessageUpdated;
    event Action<ChatMessage> OnSendMessage;
    event Action OnDeactivatePreview;
    event Action OnActivatePreview;
    
    bool IsActive { get; }
    bool IsPreview { get; }
    bool IsInputFieldFocused { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }

    void SetInputField(string text);
    void ActivatePreview();
    void DeactivatePreview();
    void Dispose();
    void Hide();
    void ResetInputField(bool loseFocus = false);
    void FocusInputField();
    void Show();
}