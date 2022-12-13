using Cysharp.Threading.Tasks;
using DCL.Social.Friends;
using System;

namespace DCl.Social.Friends
{
    public interface IFriendsApiBridge
    {
        event Action<FriendshipInitializationMessage> OnInitialized;
        event Action<string> OnFriendNotFound;
        event Action<AddFriendsPayload> OnFriendsAdded;
        [Obsolete("Old API. Use GetFriendRequestsAsync instead")]
        event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded;
        event Action<UserStatus> OnUserPresenceUpdated;
        event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated;
        event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated;
        event Action<FriendRequestPayload> OnFriendRequestReceived;

        [Obsolete("Old API. Use RejectFriendshipAsync instead")]
        void RejectFriendship(string userId);
        UniTask<RejectFriendshipPayload> RejectFriendshipAsync(string friendRequestId);
        void RemoveFriend(string userId);
        void GetFriends(int limit, int skip);
        void GetFriends(string usernameOrId, int limit);
        [Obsolete("Old API. Use GetFriendRequestsV2(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) instead")]
        void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip); // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip);
        void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip);
        [Obsolete("Old API. Use RequestFriendship(string userId, string messageBody) instead")]
        void RequestFriendship(string friendUserId);
        UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody);
        UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string friendRequestId);
        UniTask CancelRequestByUserIdAsync(string userId);
        [Obsolete("Old API. Use CancelRequestByUserIdAsync instead")]
        void CancelRequestByUserId(string userId);
        void AcceptFriendship(string userId);
        UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId);
    }
}
