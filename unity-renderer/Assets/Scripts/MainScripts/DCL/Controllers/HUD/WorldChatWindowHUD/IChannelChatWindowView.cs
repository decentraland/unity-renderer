using System;
using UnityEngine;

public interface IChannelChatWindowView
{
    event Action OnClose;
    event Action OnBack;
    
    bool IsActive { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }
    string channel { get; }

    void Dispose();
    void Hide();
    void Show();
    void Setup(string channelId, string name, string description);
}