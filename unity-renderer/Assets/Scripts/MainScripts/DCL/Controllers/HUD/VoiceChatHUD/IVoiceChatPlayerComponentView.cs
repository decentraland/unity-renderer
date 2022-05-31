using System;

public interface IVoiceChatPlayerComponentView
{
    event Action<string, bool> OnMuteUser;
    event Action<string> OnContextMenuOpen;

    void SetUserId(string userId);
    void SetUserImage(string url);
    void SetUserName(string userName);
    void SetAsMuted(bool isMuted);
    void SetAsTalking(bool isTalking);
    void SetAsBlocked(bool isBlocked);
    void SetAsFriend(bool isFriend);
    void SetBackgroundHover(bool isHover);
    void SetAsJoined(bool isJoined);
    void SetActive(bool isActive);
    void DockAndOpenUserContextMenu(UserContextMenu contextMenuPanel);
}
