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
        public bool ImTheSender { get; }

        public PublicChannelMessageNotificationModel(string messageId, string body, string channelName,
            string channelId, ulong timestamp, bool imTheSender, string username = null)
        {
            MessageId = messageId;
            Body = body;
            ChannelName = channelName;
            ChannelId = channelId;
            Timestamp = timestamp;
            ImTheSender = imTheSender;
            Username = username;
        }
    }
}
