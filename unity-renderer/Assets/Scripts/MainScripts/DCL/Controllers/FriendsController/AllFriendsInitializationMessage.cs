using System.Collections.Generic;

namespace DCL.Social.Friends
{
    public class AllFriendsInitializationMessage : FriendshipInitializationMessage
    {
        public List<string> Friends { get; }
        public List<FriendRequest> IncomingFriendRequests { get; }
        public List<FriendRequest> OutgoingFriendRequests { get; }

        public AllFriendsInitializationMessage(List<string> friends,
            List<FriendRequest> incomingFriendRequests,
            List<FriendRequest> outgoingFriendRequests)
        {
            Friends = friends;
            IncomingFriendRequests = incomingFriendRequests;
            OutgoingFriendRequests = outgoingFriendRequests;
            totalReceivedRequests = incomingFriendRequests.Count;
        }
    }
}
