using System;
using UnityEngine;

public interface IVoiceChatPlayerComponentView
{
    event Action<string, bool> OnMuteUser;

    void SetUserId(string userId);
    void SetUserImage(Texture2D texture);
    void SetUserName(string userName);
    void SetAsMuted(bool isMuted);
    void SetAsBlocked(bool isBlocked);
    void SetAsFriend(bool isFriend);
    void SetBackgroundHover(bool isHover);
}
