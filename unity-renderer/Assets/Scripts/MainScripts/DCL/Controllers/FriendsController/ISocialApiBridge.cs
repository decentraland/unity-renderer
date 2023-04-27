using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public interface ISocialApiBridge : IService
    {
        event Action<UserStatus> OnFriendAdded;
        event Action<string> OnFriendRemoved;
        event Action<FriendRequest> OnFriendRequestAdded;
        event Action<string> OnFriendRequestRemoved;
        event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        event Action<FriendRequestPayload> OnFriendRequestReceived;

        UniTask<FriendshipInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default);

        UniTask RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask<FriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody, CancellationToken cancellationToken = default);

        UniTask<AddFriendsPayload> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default);

        UniTask<AddFriendsPayload> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default);

        UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken = default);

        UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string userId, CancellationToken cancellationToken = default);

        UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask<FriendshipStatus> GetFriendshipStatusAsync(string userId, CancellationToken cancellationToken = default);
    }
}
