using System;
using UnityEngine;

public interface IPublicChatWindowView
{
    event Action OnClose;
    event Action OnBack;
    event Action<bool> OnMuteChanged;
    bool IsActive { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }
    void Dispose();
    void Hide();
    void Show();
    void Configure(PublicChatModel model);
}