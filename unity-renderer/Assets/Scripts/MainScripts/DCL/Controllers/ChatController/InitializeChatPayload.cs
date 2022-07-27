using System;

namespace DCL.Chat.WebApi
{
    [Serializable]
    public class InitializeChatPayload
    {
        public int totalUnseenMessages;
    }
}