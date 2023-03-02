using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class JoinChannelErrorPayload
    {
        public string channelId;
        public int errorCode;
    }
}