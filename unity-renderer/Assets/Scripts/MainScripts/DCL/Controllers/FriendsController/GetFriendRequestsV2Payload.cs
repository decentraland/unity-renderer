using System;

namespace DCL.Social.Friends
{
    // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
    [Serializable]
    public class GetFriendRequestsPayload
    {
        public int sentLimit;
        public int sentSkip;
        public int receivedLimit;
        public int receivedSkip;
    }

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
