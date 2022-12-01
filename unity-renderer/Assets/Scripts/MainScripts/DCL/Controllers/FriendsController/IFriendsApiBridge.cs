using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        event Action<FriendRequestPayload> OnFriendRequestAdded;

        // TODO: refactor into async promises/tasks
        void RejectFriendship(string userId);
        void RemoveFriend(string userId);
        void GetFriends(int limit, int skip);
        void GetFriends(string usernameOrId, int limit);
        void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip); // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        UniTask<AddFriendRequestsV2Payload> GetFriendRequestsV2(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip);
        void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip);
        UniTask<RequestFriendshipConfirmationPayload> RequestFriendship(string userId, string messageBody);
        void CancelRequest(string userId);
        void AcceptFriendship(string userId);
    }
}
