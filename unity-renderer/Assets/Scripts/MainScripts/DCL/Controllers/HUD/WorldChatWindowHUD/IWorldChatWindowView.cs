﻿using System;
using DCL.Interface;
using UnityEngine;

public interface IWorldChatWindowView
{
    event Action OnClose;
    event Action<string> OnOpenChat;
    
    RectTransform Transform { get; }
    bool IsActive { get; }

    void Initialize(IChatController chatController);
    void Show();
    void Hide();
    void SetDirectRecipient(UserProfile user, ChatMessage recentMessage, bool isBlocked, PresenceStatus presence);
}