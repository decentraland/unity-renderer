using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class UpdateChannelMembersPayload
    {
        public string channelId;
        public string[] members;
    }
}