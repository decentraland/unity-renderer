using System;

namespace DCL.Chat.WebApi
{
    [Serializable]
    public class UpdateTotalUnseenMessagesByChannelPayload
    {
        [Serializable]
        public class UnseenChannelMessage
        {
            public string channelId;
            public int count;
        }

        public UnseenChannelMessage[] unseenChannelMessages;
    }
}