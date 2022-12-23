using System;

namespace DCL.Chat.Notifications
{
    [Serializable]
    public class FriendRequestNotificationComponentModel : BaseComponentModel
    {
        public string FriendRequestId;
        public string UserName;
        public string UserId;
        public string Message;
        public string Time;
        public string Header;
        public bool IsAccepted;
    }
}
