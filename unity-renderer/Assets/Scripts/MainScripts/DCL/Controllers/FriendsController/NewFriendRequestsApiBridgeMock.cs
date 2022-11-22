using System;
using DCl.Social.Friends;

namespace DCL.Social.Friends
{
    public class NewFriendRequestsApiBridgeMock : IFriendsApiBridge
    {
        private readonly WebInterfaceFriendsApiBridge apiBridge;
        
        public event Action<FriendshipInitializationMessage> OnInitialized
        {
            add => apiBridge.OnInitialized += value;
            remove => apiBridge.OnInitialized -= value;
        }
        
        public event Action<string> OnFriendNotFound
        {
            add => apiBridge.OnFriendNotFound += value;
            remove => apiBridge.OnFriendNotFound -= value;
        }
        
        public event Action<AddFriendsPayload> OnFriendsAdded
        {
            add => apiBridge.OnFriendsAdded += value;
            remove => apiBridge.OnFriendsAdded -= value;
        }
        
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded
        {
            add => apiBridge.OnFriendRequestsAdded += value;
            remove => apiBridge.OnFriendRequestsAdded -= value;
        }
        
        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded
        {
            add => apiBridge.OnFriendWithDirectMessagesAdded += value;
            remove => apiBridge.OnFriendWithDirectMessagesAdded -= value;
        }
        
        public event Action<UserStatus> OnUserPresenceUpdated
        {
            add => apiBridge.OnUserPresenceUpdated += value;
            remove => apiBridge.OnUserPresenceUpdated -= value;
        }
        
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated
        {
            add => apiBridge.OnFriendshipStatusUpdated += value;
            remove => apiBridge.OnFriendshipStatusUpdated -= value;
        }
        
        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated
        {
            add => apiBridge.OnTotalFriendRequestCountUpdated += value;
            remove => apiBridge.OnTotalFriendRequestCountUpdated -= value;
        }
        
        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated
        {
            add => apiBridge.OnTotalFriendCountUpdated += value;
            remove => apiBridge.OnTotalFriendCountUpdated -= value;
        }

        public NewFriendRequestsApiBridgeMock(WebInterfaceFriendsApiBridge apiBridge)
        {
            this.apiBridge = apiBridge;
        }
        
        public void RejectFriendship(string userId)
        {
            apiBridge.RejectFriendship(userId);
        }

        public void RemoveFriend(string userId)
        {
            apiBridge.RemoveFriend(userId);
        }

        public void GetFriends(int limit, int skip)
        {
            apiBridge.GetFriends(limit, skip);
        }

        public void GetFriends(string usernameOrId, int limit)
        {
            apiBridge.GetFriends(usernameOrId, limit);
        }

        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
        {
            apiBridge.GetFriendRequests(sentLimit, sentLimit, receivedLimit, receivedSkip);
        }

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip)
        {
            apiBridge.GetFriendsWithDirectMessages(usernameOrId, limit, skip);
        }

        public void RequestFriendship(string userId)
        {
            apiBridge.RequestFriendship(userId);
        }

        public void CancelRequest(string userId)
        {
            apiBridge.CancelRequest(userId);
        }

        public void AcceptFriendship(string userId)
        {
            apiBridge.AcceptFriendship(userId);
        }
    }
}