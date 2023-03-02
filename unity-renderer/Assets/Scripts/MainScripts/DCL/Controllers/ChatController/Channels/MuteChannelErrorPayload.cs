using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class MuteChannelErrorPayload
    {
        public string channelId;
        public int errorCode;
    }
}