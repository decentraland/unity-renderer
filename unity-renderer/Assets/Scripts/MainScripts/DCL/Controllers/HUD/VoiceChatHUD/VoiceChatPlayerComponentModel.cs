using System;
using UnityEngine;

[Serializable]
public class VoiceChatPlayerComponentModel : BaseComponentModel
{
    public string userId;
    public Texture2D userImage;
    public string userName;
    public bool isMuted = false;
    public bool isBlocked = false;
    public bool isFriend = false;
    public bool isBackgroundHover = false;
}