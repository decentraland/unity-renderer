using System;
using UnityEngine;

[Serializable]
public class ChatNotificationMessageComponentModel : BaseComponentModel
{
    public int maxHeaderCharacters;
    public int maxContentCharacters;
    public int maxSenderCharacters;

    public string message;
    public string time;
    public string messageHeader;
    public string messageSender;
    public bool isPrivate;
    public string imageUri;
    public string notificationTargetId;
}
