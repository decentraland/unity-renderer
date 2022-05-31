using System;
using UnityEngine;

public interface IVoiceChatWindowComponentView
{
    event Action OnClose;
    event Action<bool> OnJoinVoiceChat;
    event Action<string> OnAllowUsersFilterChange;
    event Action OnGoToCrowd;

    RectTransform Transform { get; }
    UserContextMenu ContextMenuPanel { get; }

    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetNumberOfPlayers(int numPlayers);
    void SetAsJoined(bool isJoined);
    VoiceChatPlayerComponentView CreateNewPlayerInstance();
}