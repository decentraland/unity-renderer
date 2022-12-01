using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class GetFriendRequestsV2Payload
    {
        public string messageId;
        public int sentLimit;
        public int sentSkip;
        public int receivedLimit;
        public int receivedSkip;
    }
}
