using System;

namespace DCL.Friends.WebApi
{
    [Serializable]
    public class UpdateTotalFriendRequestsPayload
    {
        public int totalReceivedRequests;
        public int totalSentRequests;
    }
}