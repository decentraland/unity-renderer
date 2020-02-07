using System.Collections.Generic;

public class NotificationModel
{
    public enum NotificationType
    {
        GENERIC,
        SCRIPTING_ERROR,
        COMMS_ERROR,
        AIRDROPPING,
        GENERIC_WITHOUT_BUTTON
    }

    public NotificationType type;
    public string message;
    public string buttonMessage;
    public float timer;
    public string scene;
    public System.Action callback;
    public string externalCallbackID;
}