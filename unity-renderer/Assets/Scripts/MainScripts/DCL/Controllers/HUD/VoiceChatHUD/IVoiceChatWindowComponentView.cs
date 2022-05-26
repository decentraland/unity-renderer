using System;
using UnityEngine;

public interface IVoiceChatWindowComponentView
{
    event Action OnClose;
    event Action<bool> OnJoinVoiceChat;
    event Action<string> OnAllowUsersFilterChange;
    event Action<string, bool> OnRequestMuteUser;

    RectTransform Transform { get; }

    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetNumberOfPlayers(int numPlayers);
    void AddOrUpdatePlayer(Player player);
    void RemoveUser(string userId);
    void SetUserRecording(string userId, bool isRecording);
    void SetUserMuted(string userId, bool isMuted);
    void SetUserBlocked(string userId, bool blocked);
}