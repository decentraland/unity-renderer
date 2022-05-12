using System;
using UnityEngine;

public interface IChannelChatWindowView
{
    event Action OnClose;
    event Action OnBack;
    event Action<bool> OnFocused;

    bool IsActive { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }
    bool IsFocused { get; }

    void Dispose();
    void Hide();
    void Show();
    void Configure(PublicChatChannelModel model);
    void ActivatePreview();
    void DeactivatePreview();
}