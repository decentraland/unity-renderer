using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class AddFriendRequestsPayload
    {
        public string[] requestedTo;
        public string[] requestedFrom;
        public int totalReceivedFriendRequests;
        public int totalSentFriendRequests;
    }
}