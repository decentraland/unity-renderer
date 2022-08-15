using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class ChannelInfoPayload
    {
        public string channelId;
        public string description;
        public int unseenMessages;
        public int memberCount;
        public bool joined;
        public bool muted;
    }
}