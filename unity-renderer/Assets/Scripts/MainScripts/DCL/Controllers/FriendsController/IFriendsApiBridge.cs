using System;
using DCL.Social.Friends;

namespace DCl.Social.Friends
{
    public interface IFriendsApiBridge
    {
        event Action<FriendshipInitializationMessage> OnInitialized;
        event Action<string> OnFriendNotFound;
        event Action<AddFriendsPayload> OnFriendsAdded;
        event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded;
        event Action<UserStatus> OnUserPresenceUpdated;
        event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated;
        event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated;
        
        // TODO: refactor into async promises/tasks
        void RejectFriendship(string userId);
        void RemoveFriend(string userId);
        void GetFriends(int limit, int skip);
        void GetFriends(string usernameOrId, int limit);
        void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip);
        void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip);
        void RequestFriendship(string userId);
        void CancelRequest(string userId);
        void AcceptFriendship(string userId);
    }
}