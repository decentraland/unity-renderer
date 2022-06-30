using System;
using UnityEngine;

[Serializable]
public class VoiceChatPlayerComponentModel : BaseComponentModel
{
    public string userId;
    public string userImageUrl;
    public string userName;
    public bool isMuted = false;
    public bool isTalking = false;
    public bool isBlocked = false;
    public bool isFriend = false;
    public bool isBackgroundHover = false;
    public bool isJoined = false;
}