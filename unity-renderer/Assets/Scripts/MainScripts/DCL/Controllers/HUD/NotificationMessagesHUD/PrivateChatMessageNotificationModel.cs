using JetBrains.Annotations;

namespace DCL.Chat.Notifications
{
    public class PrivateChatMessageNotificationModel
    {
        public string MessageId { get; }
        public string SenderId { get; }
        public string Body { get; }
        public ulong Timestamp { get; }
        public string SenderUsername { get; }
        public string PeerUsername { get; }
        public string ProfilePicture { get; }
        public bool ImTheSender { get; }

        public PrivateChatMessageNotificationModel(string messageId, string senderId, string body, ulong timestamp, string senderUsername, string peerUsername,
            bool imTheSender, string profilePicture = null)
        {
            MessageId = messageId;
            SenderId = senderId;
            Body = body;
            Timestamp = timestamp;
            SenderUsername = senderUsername;
            PeerUsername = peerUsername;
            ImTheSender = imTheSender;
            ProfilePicture = profilePicture;
        }
    }
}
