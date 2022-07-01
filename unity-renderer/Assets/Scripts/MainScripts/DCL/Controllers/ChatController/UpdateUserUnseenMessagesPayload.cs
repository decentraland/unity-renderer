using System;

namespace DCL.Chat.WebApi
{
    [Serializable]
    public class UpdateUserUnseenMessagesPayload
    {
        public string userId;
        public int total;
    }
}