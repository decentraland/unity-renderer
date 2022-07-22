using System;

namespace DCL.Chat.WebApi
{
    [Serializable]
    public class UpdateChannelUnseenMessagesPayload
    {
        public string channelId;
        public int total;
    }
}