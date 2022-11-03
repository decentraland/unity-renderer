using JetBrains.Annotations;

namespace DCL.Chat.Notifications
{
    public class PublicChannelMessageNotificationModel
    {
        public string MessageId { get; }
        public string Body { get; }
        public string ChannelName { get; }
        public string ChannelId { get; }
        public ulong Timestamp { get; }
        [CanBeNull] public string Username { get; }
        
        public PublicChannelMessageNotificationModel(string messageId, string body, string channelName, string channelId, ulong timestamp, string username = null)
        {
            MessageId = messageId;
            Body = body;
            ChannelName = channelName;
            ChannelId = channelId;
            Timestamp = timestamp;
            Username = username;
        }
    }
}