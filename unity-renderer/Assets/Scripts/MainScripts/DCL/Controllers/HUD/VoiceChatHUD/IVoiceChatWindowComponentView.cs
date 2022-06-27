using DCL.SettingsCommon;
using System;
using UnityEngine;

public interface IVoiceChatWindowComponentView
{
    event Action OnClose;
    event Action<bool> OnJoinVoiceChat;
    event Action<string> OnAllowUsersFilterChange;
    event Action OnGoToCrowd;
    event Action<bool> OnMuteAll;
    event Action<string, bool> OnMuteUser;

    RectTransform Transform { get; }
    bool isMuteAllOn { get; }
    int numberOfPlayers { get; }
    int numberOfPlayersTalking { get; }

    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetNumberOfPlayers(int numPlayers);
    void SetAsJoined(bool isJoined);
    void SelectAllowUsersOption(int optionIndex);
    void SetPlayerMuted(string usersId, bool isMuted);
    void SetPlayerRecording(string userId, bool isRecording);
    void SetPlayerBlocked(string userId, bool isBlocked);
    void SetPlayerAsFriend(string userId, bool isFriend);
    void SetPlayerAsJoined(string userId, bool isJoined);
    void AddOrUpdatePlayer(UserProfile player);
    void RemoveUser(string userId);
    string GetUserTalkingByIndex(int index);
}