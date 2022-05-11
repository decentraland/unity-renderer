using System;
using UnityEngine;

public interface IChannelChatWindowView
{
    event Action OnClose;
    event Action OnBack;

    bool IsActive { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }

    void Dispose();
    void Hide();
    void Show();
    void Configure(PublicChatChannelModel model);
    void ActivatePreview();
    void DeactivatePreview();
}