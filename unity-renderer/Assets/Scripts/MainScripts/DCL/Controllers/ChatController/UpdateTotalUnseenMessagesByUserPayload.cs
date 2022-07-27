using System;
using System.Collections.Generic;

namespace DCL.Chat.WebApi
{
    [Serializable]
    public class UpdateTotalUnseenMessagesByUserPayload
    {
        public Dictionary<string, int> unseenPrivateMessages;
    }
}