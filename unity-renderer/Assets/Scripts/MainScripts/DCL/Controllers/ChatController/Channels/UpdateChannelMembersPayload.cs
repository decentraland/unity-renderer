using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class ChannelMember
    {
        public string userId;
        public bool isOnline;
    }

    [Serializable]
    public class UpdateChannelMembersPayload
    {
        public string channelId;
        public ChannelMember[] members;
    }
}