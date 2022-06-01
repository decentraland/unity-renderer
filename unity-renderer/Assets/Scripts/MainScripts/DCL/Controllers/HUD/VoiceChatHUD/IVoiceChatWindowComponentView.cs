using System;
using UnityEngine;

public interface IVoiceChatWindowComponentView
{
    event Action OnClose;
    event Action<bool> OnJoinVoiceChat;
    event Action<string> OnAllowUsersFilterChange;
    event Action OnGoToCrowd;
    event Action<bool> OnMuteAll;

    RectTransform Transform { get; }
    UserContextMenu ContextMenuPanel { get; }

    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetNumberOfPlayers(int numPlayers);
    void SetAsJoined(bool isJoined);
    void SetMuteAllIsOn(bool isOn, bool notify = true);
    VoiceChatPlayerComponentView CreateNewPlayerInstance();
}