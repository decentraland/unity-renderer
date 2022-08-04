using System;

namespace DCL.Chat.WebApi
{
    [Serializable]
    public class UpdateTotalUnseenMessagesByUserPayload
    {
        [Serializable]
        public class UnseenPrivateMessage
        {
            public string userId;
            public int count;
        }
        
        public UnseenPrivateMessage[] unseenPrivateMessages;
    }
}