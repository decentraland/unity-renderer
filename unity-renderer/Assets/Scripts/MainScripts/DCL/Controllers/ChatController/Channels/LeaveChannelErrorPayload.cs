using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class LeaveChannelErrorPayload
    {
        public string channelId;
        public string message;
    }
}