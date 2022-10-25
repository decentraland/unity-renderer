using JetBrains.Annotations;

namespace DCL.Chat.Notifications
{
    public class PrivateChatMessageNotificationModel
    {
        public string MessageId { get; }
        public string SenderId { get; }
        public string Body { get; }
        public ulong Timestamp { get; }
        [CanBeNull] public string Username { get; }
        [CanBeNull] public string ProfilePicture { get; }
        
        public PrivateChatMessageNotificationModel(string messageId, string senderId, string body, ulong timestamp, string username = null, string profilePicture = null)
        {
            MessageId = messageId;
            SenderId = senderId;
            Body = body;
            Timestamp = timestamp;
            Username = username;
            ProfilePicture = profilePicture;
        }
    }
}