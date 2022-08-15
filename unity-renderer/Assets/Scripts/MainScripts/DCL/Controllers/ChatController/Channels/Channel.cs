namespace DCL.Chat.Channels
{
    public class Channel
    {
        public string ChannelId { get; internal set; }
        public int UnseenMessages { get; internal set; }
        public int MemberCount { get; internal set; }
        public bool Joined { get; internal set; }
        public bool Muted { get; internal set; }
        public string Name => ChannelId;
        public string Description { get; internal set; }
        public long LastMessageTimestamp { get; internal set; }

        public Channel(string channelId, int unseenMessages, int memberCount, bool joined, bool muted, string description, long lastMessageTimestamp)
        {
            ChannelId = channelId;
            UnseenMessages = unseenMessages;
            MemberCount = memberCount;
            Joined = joined;
            Muted = muted;
            Description = description;
            LastMessageTimestamp = lastMessageTimestamp;
        }

        public void CopyFrom(Channel channel)
        {
            ChannelId = channel.ChannelId;
            UnseenMessages = channel.UnseenMessages;
            MemberCount = channel.MemberCount;
            Joined = channel.Joined;
            Muted = channel.Muted;
            LastMessageTimestamp = channel.LastMessageTimestamp;
        }
    }
}