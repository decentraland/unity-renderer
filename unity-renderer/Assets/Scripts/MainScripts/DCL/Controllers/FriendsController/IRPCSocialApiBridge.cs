using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using DCL.Social.Friends;
using Decentraland.Renderer.RendererServices;
using Decentraland.Social.Friendships;
using RPC;
using System;
using System.Threading;

namespace MainScripts.DCL.Controllers.FriendsController
{
    public interface IRPCSocialApiBridge
    {
        event Action<UserStatus> OnFriendAdded;
        event Action<string> OnFriendRemoved;
        event Action<FriendRequest> OnFriendRequestAdded;
        event Action<string> OnFriendRequestRemoved;
        event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        event Action<FriendRequestPayload> OnFriendRequestReceived;

        UniTaskVoid InitializeClient(CancellationToken cancellationToken = default);

        UniTaskVoid InitializeSubscriptions(CancellationToken cancellationToken = default);

        UniTask<FriendshipInitializationMessage> InitializeFriendshipsInformation(CancellationToken cancellationToken = default);

        UniTask ListenToFriendEvents(CancellationToken cancellationToken = default);

        UniTask RejectFriendship(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask<FriendRequest> RequestFriendship(string friendUserId, string messageBody, CancellationToken cancellationToken = default);

        UniTask UpdateFriendship(UpdateFriendshipPayload updateFriendshipPayload, CancellationToken cancellationToken = default);

        void RemoveFriend(string userId);

        UniTask<AddFriendsPayload> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default);

        UniTask<AddFriendsPayload> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default);

        void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip);

        UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken = default);

        void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip);

        UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody, CancellationToken cancellationToken = default);

        UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask CancelRequestByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        void CancelRequestByUserId(string userId);

        void AcceptFriendship(string userId);

        UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken = default);

        UniTask<ApproveFriendRequestReply> ApproveFriendRequest(ApproveFriendRequestPayload request, RPCContext context, CancellationToken ct);

        UniTask<RejectFriendRequestReply> RejectFriendRequest(RejectFriendRequestPayload request, RPCContext context, CancellationToken ct);

        UniTask<CancelFriendRequestReply> CancelFriendRequest(CancelFriendRequestPayload request, RPCContext context, CancellationToken ct);

        UniTask<ReceiveFriendRequestReply> ReceiveFriendRequest(ReceiveFriendRequestPayload request, RPCContext context, CancellationToken ct);

        FriendRequestErrorCodes ToErrorCode(FriendshipErrorCode code);
    }
}
