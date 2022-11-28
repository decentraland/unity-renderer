using System;

namespace DCL.Interface
{
    [Serializable]
    public class GetFriendRequestsPayload
    {
        public string messageId;
        public int sentLimit;
        public int sentSkip;
        public int receivedLimit;
        public int receivedSkip;
    }
}