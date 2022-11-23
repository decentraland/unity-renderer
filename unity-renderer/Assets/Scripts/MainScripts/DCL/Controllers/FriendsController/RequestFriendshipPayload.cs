using System;

namespace DCL.Interface
{
    [Serializable]
    public class RequestFriendshipPayload
    {
        public string messageId;
        public string userId;
        public string messageBody;
    }
}