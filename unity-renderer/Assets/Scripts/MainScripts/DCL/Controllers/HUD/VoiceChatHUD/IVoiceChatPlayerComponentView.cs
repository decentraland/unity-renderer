using System;

public interface IVoiceChatPlayerComponentView
{
    event Action<string, bool> OnMuteUser;

    void SetUserId(string userId);
    void SetUserImage(string url);
    void SetUserName(string userName);
    void SetAsMuted(bool isMuted);
    void SetAsTalking(bool isTalking);
    void SetAsBlocked(bool isBlocked);
    void SetAsFriend(bool isFriend);
    void SetBackgroundHover(bool isHover);
}
