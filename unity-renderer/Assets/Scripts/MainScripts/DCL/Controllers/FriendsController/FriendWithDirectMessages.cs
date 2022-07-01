using System;

namespace DCL.Friends.WebApi
{
    [Serializable]
    public class FriendWithDirectMessages
    {
        public string userId;
        public string lastMessageBody;
        public long lastMessageTimestamp;
    }
}