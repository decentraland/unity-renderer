using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class ChannelMember
    {
        public string userId;
        public bool isOnline;
        public string name;
    }

    [Serializable]
    public class UpdateChannelMembersPayload
    {
        public string channelId;
        public ChannelMember[] members;
    }
}