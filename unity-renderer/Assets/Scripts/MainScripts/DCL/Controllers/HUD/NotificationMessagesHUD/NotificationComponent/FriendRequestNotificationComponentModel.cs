using System;

[Serializable]
public class FriendRequestNotificationComponentModel : BaseComponentModel
{
    public string UserName;
    public string UserId;
    public string Message;
    public string Time;
    public string Header;
    public string ImageUri;
    public bool IsAccepted;
}
