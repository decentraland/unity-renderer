using System;
using UnityEngine;

public interface IVoiceChatWindowComponentView
{
    event Action OnClose;
    event Action<bool> OnJoinVoiceChat;
    event Action<string> OnAllowUsersFilterChange;

    RectTransform Transform { get; }

    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetNumberOfPlayers(int numPlayers);
    void SetEmptyListActive(bool isActive);
    VoiceChatPlayerComponentView CreateNewPlayerInstance();
}