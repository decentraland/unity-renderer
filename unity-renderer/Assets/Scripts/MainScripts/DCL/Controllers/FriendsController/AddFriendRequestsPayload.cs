using System;

namespace DCL.Friends.WebApi
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