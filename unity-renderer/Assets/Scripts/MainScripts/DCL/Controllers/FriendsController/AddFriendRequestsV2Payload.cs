using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class AddFriendRequestsV2Payload
    {
        public string messageId;
        public FriendRequestPayload[] requestedTo;
        public FriendRequestPayload[] requestedFrom;
        public int totalReceivedFriendRequests;
        public int totalSentFriendRequests;
    }
}
