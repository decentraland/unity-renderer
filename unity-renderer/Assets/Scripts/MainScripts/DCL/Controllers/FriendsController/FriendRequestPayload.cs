using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class FriendRequestPayload
    {
        public string friendRequestId;
        public long timestamp;
        public string from;
        public string to;
        public string messageBody;
    }
}