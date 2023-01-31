using Cysharp.Threading.Tasks;
using DCL.Social.Friends;
using System;
using System.Threading;

namespace DCl.Social.Friends
{
    public interface IFriendsApiBridge
    {
        event Action<FriendshipInitializationMessage> OnInitialized;
        event Action<string> OnFriendNotFound;
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
        UniTask<RejectFriendshipPayload> RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);
        void RemoveFriend(string userId);
        UniTask<AddFriendsPayload> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default);
        UniTask<AddFriendsPayload> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default);
        [Obsolete("Old API. Use GetFriendRequestsV2(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) instead")]
        void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip); // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken = default);
        void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip);
        [Obsolete("Old API. Use RequestFriendship(string userId, string messageBody) instead")]
        void RequestFriendship(string friendUserId);
        UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody, CancellationToken cancellationToken = default);
        UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken = default);
        [Obsolete("Will be removed in the future")]
        UniTask CancelRequestByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        [Obsolete("Will be removed in the future")]
        void CancelRequestByUserId(string userId);
        void AcceptFriendship(string userId);
        UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);
        UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken = default);
    }
}
