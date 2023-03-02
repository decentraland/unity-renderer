namespace DCL.Chat.Notifications
{
    public class FriendRequestNotificationModel
    {
        public string FriendRequestId { get; }
        public string UserId { get; }
        public string UserName { get; }
        public string Header { get; }
        public string Message { get; }
        public ulong Timestamp { get; }
        public bool IsAccepted { get; }

        public FriendRequestNotificationModel(string friendRequestId, string userId, string userName, string header, string message, ulong timestamp, bool isAccepted)
        {
            FriendRequestId = friendRequestId;
            UserId = userId;
            UserName = userName;
            Header = header;
            Message = message;
            Timestamp = timestamp;
            IsAccepted = isAccepted;
        }
    }
}
