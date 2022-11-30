using JetBrains.Annotations;

namespace DCL.Chat.Notifications
{
    public class FriendRequestNotificationModel
    {
        public string UserId { get; }
        public string UserName { get; }
        public string Header { get; }
        public string Message { get; }
        public ulong Timestamp { get; }
        [CanBeNull] public string ProfilePicture { get; }
        public bool IsAccepted { get; }

        public FriendRequestNotificationModel(string UserId, string UserName, string Header, string Message, ulong Timestamp, string ProfilePicture, bool IsAccepted)
        {
            this.UserId = UserId;
            this.UserName = UserName;
            this.Header = Header;
            this.Message = Message;
            this.Timestamp = Timestamp;
            this.ProfilePicture = ProfilePicture;
            this.IsAccepted = IsAccepted;
        }
    }
}
