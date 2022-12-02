using System;

namespace DCl.Social.Friends
{
    [Serializable]
    public class FriendWithDirectMessages
    {
        public string userId;
        public string lastMessageBody;
        public long lastMessageTimestamp;
    }
}