using System;
using UnityEngine;

[Serializable]
public class ChatNotificationMessageComponentModel : BaseComponentModel
{
    public int maxHeaderCharacters;
    public int maxContentCharacters;

    public string message;
    public string time;
    public string messageHeader;
    public bool isPrivate;
    public Sprite profileIcon;
    public string notificationTargetId;
}
