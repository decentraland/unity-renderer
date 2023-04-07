using Cysharp.Threading.Tasks;
using DCL;
using DCl.Social.Friends;
using DCL.Social.Friends;
using Decentraland.Renderer.RendererServices;
using Decentraland.Social.Friendships;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using NSubstitute.ReceivedExtensions;
using RPC;
using System;
using System.Threading;
using rpc_csharp;
using rpc_csharp.transport;
using System.Collections.Generic;
using UnityEngine;
using Payload = Decentraland.Social.Friendships.Payload;

namespace MainScripts.DCL.Controllers.FriendsController
{
    public class RPCSocialApiBridge : IRPCSocialApiBridge
    {
        private const int REQUEST_TIMEOUT = 30;

        private string accessToken;

        private IRPC rpc;

        public event Action<UserStatus> OnFriendAdded;
        public event Action<string> OnFriendRemoved;
        public event Action<FriendRequest> OnIncomingFriendRequestAdded;
        public event Action<FriendRequest> OnOutgoingFriendRequestAdded;
        public event Action<FriendshipUpdateStatusMessage> OnReceivedFriendshipEvent;
        public event Action<string> OnFriendRequestRemoved;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestReceived;

        public RPCSocialApiBridge(IRPC rpc, MatrixInitializationBridge matrixInitializationBridge)
        {
            this.rpc = rpc;

            matrixInitializationBridge.OnReceiveMatrixAccessToken += (token) => { this.accessToken = token;};
        }

        public async UniTaskVoid InitializeClient(CancellationToken cancellationToken = default)
        {
            // start listening to streams
            // await UniTask.WhenAny(this.ListenToFriendEvents(cancellationToken));
        }

        public async UniTask<FriendshipInitializationMessage> InitializeFriendshipsInformation(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ownUserProfile = UserProfile.GetOwnUserProfile();

            await UniTask.WaitUntil(() =>
                    !string.IsNullOrEmpty(ownUserProfile.userId) && !string.IsNullOrEmpty(this.accessToken),
                PlayerLoopTiming.Update,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var friendsStream = rpc.Social().GetFriends(new Payload() { SynapseToken = this.accessToken });

            await foreach (var friends in friendsStream.WithCancellation(cancellationToken))
            {
                foreach (var friend in friends.Users_) { OnFriendAdded?.Invoke(new UserStatus { userId = friend.Address }); }
            }

            cancellationToken.ThrowIfCancellationRequested();

            // var requestEvents = await rpc.Social().GetRequestEvents(new Empty());
            //
            // foreach (var friendRequest in requestEvents.Incoming.Items)
            // {
            //     OnIncomingFriendRequestAdded?.Invoke(new FriendRequest(
            //         GetFriendRequestId(friendRequest.User.Address, friendRequest.CreatedAt), friendRequest.CreatedAt, friendRequest.User.Address, ownUserProfile.userId, friendRequest.Message));
            // }
            //
            // foreach (var friendRequest in requestEvents.Outgoing.Items)
            // {
            //     OnOutgoingFriendRequestAdded?.Invoke(new FriendRequest(
            //         GetFriendRequestId(friendRequest.User.Address, friendRequest.CreatedAt), friendRequest.CreatedAt, ownUserProfile.userId, friendRequest.User.Address, friendRequest.Message));
            // }

            return new FriendshipInitializationMessage()
            {
                // totalReceivedRequests = requestEvents.Incoming.Items.Count,
                totalReceivedRequests = 0
            };
        }

        public async UniTask ListenToFriendEvents(CancellationToken cancellationToken = default)
        {
            var ownUserProfile = UserProfile.GetOwnUserProfile();

            await UniTask.WaitUntil(() => ownUserProfile.userId != null, PlayerLoopTiming.Update, cancellationToken);

            var stream = rpc.Social().SubscribeFriendshipEventsUpdates(new Payload() { SynapseToken = "" });

            cancellationToken.ThrowIfCancellationRequested();

            await foreach (var item in stream.WithCancellation(cancellationToken))
            {
                foreach (var friendshipEvent in item.Events)
                {
                    var action = ToFriendshipAction(friendshipEvent.BodyCase);

                    OnReceivedFriendshipEvent?.Invoke(new FriendshipUpdateStatusMessage()
                    {
                        // TODO: Get User Id from friendshipEvent
                        userId = "get userId",
                        action = ToFriendshipAction(friendshipEvent.BodyCase)
                    });
                }
            }
        }

        public async UniTask RejectFriendship(string friendRequestId, CancellationToken cancellationToken = default)
        {
            string userId = GetUserIdFromFriendRequestId(friendRequestId);

            var updateFriendshipPayload = new UpdateFriendshipPayload()
            {
                Event =
                    new FriendshipEventPayload()
                    {
                        Reject = new RejectPayload()
                        {
                            User = new User() { Address = userId }
                        }
                    }
            };

            await this.UpdateFriendship(updateFriendshipPayload, cancellationToken);

            OnFriendRequestRemoved?.Invoke(userId);
        }

        public async UniTask<FriendRequest> RequestFriendship(string friendUserId, string messageBody, CancellationToken cancellationToken = default)
        {
            long createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var updateFriendshipPayload = new UpdateFriendshipPayload()
            {
                Event =
                {
                    Request = new RequestPayload()
                    {
                        Message = messageBody,
                        User = new User() { Address = friendUserId },
                    }
                }
            };

            await UpdateFriendship(updateFriendshipPayload, cancellationToken);

            return new FriendRequest(GetFriendRequestId(friendUserId, createdAt), createdAt, "GET OWN ADDRESS", friendUserId, messageBody);
        }

        private static string GetUserIdFromFriendRequestId(string friendRequestId) =>
            friendRequestId.Split("-")[0];

        private static string GetFriendRequestId(string userId, long createdAt) =>
            $"{userId}-{createdAt}";

        public async UniTask UpdateFriendship(UpdateFriendshipPayload updateFriendshipPayload, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                var response = await rpc.Social()
                                        .UpdateFriendshipEvent(updateFriendshipPayload)
                                        .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT));

                switch (response.ResponseCase)
                {
                    case UpdateFriendshipResponse.ResponseOneofCase.Event:
                        break;
                    case UpdateFriendshipResponse.ResponseOneofCase.Error:
                        var error = response.Error;
                        throw new FriendshipException(ToErrorCode(error));
                    case UpdateFriendshipResponse.ResponseOneofCase.None:
                    default:
                        throw new InvalidCastException();
                }

                await UniTask.SwitchToMainThread(cancellationToken);
            }
            finally { await UniTask.SwitchToMainThread(); }
        }

        public void RemoveFriend(string userId)
        {
            throw new NotImplementedException();
        }

        public UniTask<AddFriendsPayload> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<AddFriendsPayload> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
        {
            throw new NotImplementedException();
        }

        public UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip)
        {
            throw new NotImplementedException();
        }

        public UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask CancelRequestByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public void CancelRequestByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public void AcceptFriendship(string userId)
        {
            throw new NotImplementedException();
        }

        public UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<ApproveFriendRequestReply> ApproveFriendRequest(ApproveFriendRequestPayload request, RPCContext context, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask<RejectFriendRequestReply> RejectFriendRequest(RejectFriendRequestPayload request, RPCContext context, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask<CancelFriendRequestReply> CancelFriendRequest(CancelFriendRequestPayload request, RPCContext context, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask<ReceiveFriendRequestReply> ReceiveFriendRequest(ReceiveFriendRequestPayload request, RPCContext context, CancellationToken ct) =>
            throw new NotImplementedException();

        public FriendRequestErrorCodes ToErrorCode(FriendshipErrorCode code)
        {
            switch (code)
            {
                default:
                case FriendshipErrorCode.Unknown:
                    return FriendRequestErrorCodes.Unknown;
                case FriendshipErrorCode.BlockedUser:
                    return FriendRequestErrorCodes.BlockedUser;
                case FriendshipErrorCode.InvalidRequest:
                    return FriendRequestErrorCodes.InvalidRequest;
                case FriendshipErrorCode.NonExistingUser:
                    return FriendRequestErrorCodes.NonExistingUser;
                case FriendshipErrorCode.NotEnoughTimePassed:
                    return FriendRequestErrorCodes.NotEnoughTimePassed;
                case FriendshipErrorCode.TooManyRequestsSent:
                    return FriendRequestErrorCodes.TooManyRequestsSent;
            }
        }

        private static FriendshipAction ToFriendshipAction(FriendshipEventResponse.BodyOneofCase body, bool sent_request = false)
        {
            switch (body)
            {
                case FriendshipEventResponse.BodyOneofCase.None:
                    return FriendshipAction.NONE;
                case FriendshipEventResponse.BodyOneofCase.Request:
                    return sent_request ? FriendshipAction.REQUESTED_TO : FriendshipAction.REQUESTED_FROM;
                case FriendshipEventResponse.BodyOneofCase.Accept:
                    return FriendshipAction.APPROVED;
                case FriendshipEventResponse.BodyOneofCase.Reject:
                    return FriendshipAction.REJECTED;
                case FriendshipEventResponse.BodyOneofCase.Delete:
                    return FriendshipAction.DELETED;
                case FriendshipEventResponse.BodyOneofCase.Cancel:
                    return FriendshipAction.CANCELLED;
                default: throw new ArgumentOutOfRangeException(nameof(body), body, null);
            }
        }
    }
}
