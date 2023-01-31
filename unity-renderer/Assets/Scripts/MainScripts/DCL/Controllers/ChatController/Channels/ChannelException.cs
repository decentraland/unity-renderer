using System;

namespace DCL.Chat.Channels
{
    public class ChannelException : Exception
    {
        public string ChannelId { get; }
        public ChannelErrorCode ErrorCode { get; }

        public ChannelException(string channelId, ChannelErrorCode errorCode)
        {
            ChannelId = channelId;
            ErrorCode = errorCode;
        }
    }
}
