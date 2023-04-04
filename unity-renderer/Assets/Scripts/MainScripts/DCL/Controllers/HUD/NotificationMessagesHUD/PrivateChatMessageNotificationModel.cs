namespace DCL.Chat.Notifications
{
    public class PrivateChatMessageNotificationModel
    {
        public string MessageId { get; }
        public string TargetId { get; }
        public string Body { get; }
        public ulong Timestamp { get; }
        public string SenderUsername { get; }
        public string PeerUsername { get; }
        public string ProfilePicture { get; }
        public bool ImTheSender { get; }
        public bool IsOwnPlayerMentioned { get; }

        public PrivateChatMessageNotificationModel(string messageId, string targetId, string body, ulong timestamp, string senderUsername, string peerUsername,
            bool imTheSender, bool isOwnPlayerMentioned, string profilePicture = null)
        {
            MessageId = messageId;
            TargetId = targetId;
            Body = body;
            Timestamp = timestamp;
            SenderUsername = senderUsername;
            PeerUsername = peerUsername;
            ImTheSender = imTheSender;
            IsOwnPlayerMentioned = isOwnPlayerMentioned;
            ProfilePicture = profilePicture;
        }
    }
}
