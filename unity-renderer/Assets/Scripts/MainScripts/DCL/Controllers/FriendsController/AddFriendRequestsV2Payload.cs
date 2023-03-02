using System;

namespace DCL.Social.Friends
{
    // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
    [Serializable]
    public class AddFriendRequestsPayload
    {
        public string[] requestedTo;
        public string[] requestedFrom;
        public int totalReceivedFriendRequests;
        public int totalSentFriendRequests;
    }

    [Serializable]
    public class AddFriendRequestsV2Payload
    {
        public FriendRequestPayload[] requestedTo;
        public FriendRequestPayload[] requestedFrom;
        public int totalReceivedFriendRequests;
        public int totalSentFriendRequests;
    }
}
