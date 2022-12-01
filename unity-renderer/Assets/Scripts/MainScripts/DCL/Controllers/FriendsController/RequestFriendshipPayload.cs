using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class RequestFriendshipPayload
    {
        public string messageId;
        public string userId;
        public string messageBody;
    }
}
