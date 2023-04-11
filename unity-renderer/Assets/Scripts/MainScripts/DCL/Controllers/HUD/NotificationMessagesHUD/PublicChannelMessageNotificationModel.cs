namespace DCL.Chat.Notifications
{
    public class PublicChannelMessageNotificationModel
    {
        public string MessageId { get; }
        public string Body { get; }
        public string ChannelName { get; }
        public string ChannelId { get; }
        public ulong Timestamp { get; }
        public string Username { get; }
        public bool ImTheSender { get; }
        public bool IsOwnPlayerMentioned { get; }
        public bool ShouldPlayMentionSfx { get; }

        public PublicChannelMessageNotificationModel(string messageId, string body, string channelName,
            string channelId, ulong timestamp, bool imTheSender, string username, bool isOwnPlayerMentioned,
            bool shouldPlayMentionSfx)
        {
            MessageId = messageId;
            Body = body;
            ChannelName = channelName;
            ChannelId = channelId;
            Timestamp = timestamp;
            ImTheSender = imTheSender;
            Username = username;
            IsOwnPlayerMentioned = isOwnPlayerMentioned;
            ShouldPlayMentionSfx = shouldPlayMentionSfx;
        }
    }
}
