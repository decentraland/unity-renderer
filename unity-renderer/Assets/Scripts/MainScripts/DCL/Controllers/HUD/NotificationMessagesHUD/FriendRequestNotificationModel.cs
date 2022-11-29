using JetBrains.Annotations;

namespace DCL.Chat.Notifications
{
    public class FriendRequestNotificationModel
    {
        public string SenderId { get; }
        public string SenderName { get; }
        public string Header { get; }
        public string Body { get; }
        public ulong Timestamp { get; }
        [CanBeNull] public string ProfilePicture { get; }

        public FriendRequestNotificationModel(string SenderId, string SenderName, string Header, string Body, ulong Timestamp, string ProfilePicture)
        {
            this.SenderId = SenderId;
            this.SenderName = SenderName;
            this.Header = Header;
            this.Body = Body;
            this.Timestamp = Timestamp;
            this.ProfilePicture = ProfilePicture;
        }
    }
}
