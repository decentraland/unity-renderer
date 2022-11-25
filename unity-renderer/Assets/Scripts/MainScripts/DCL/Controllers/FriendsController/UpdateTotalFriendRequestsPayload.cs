using System;

namespace DCl.Social.Friends
{
    [Serializable]
    public class UpdateTotalFriendRequestsPayload
    {
        public int totalReceivedRequests;
        public int totalSentRequests;
    }
}