namespace DCL.Chat.Channels
{
    public static class ChannelExtensions
    {
        public static Channel ToChannel(this ChannelInfoPayload payload) =>
            new (payload.channelId, payload.name, payload.unseenMessages,
                payload.memberCount,
                payload.joined, payload.muted, payload.description);
    }
}
