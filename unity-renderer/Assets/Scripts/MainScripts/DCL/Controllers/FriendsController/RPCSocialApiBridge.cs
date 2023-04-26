using Cysharp.Threading.Tasks;
using DCL;
using DCl.Social.Friends;
using Decentraland.Renderer.RendererServices;
using Decentraland.Social.Friendships;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using MainScripts.DCL.Controllers.FriendsController;
using RPC;
using System;
using System.Threading;
using rpc_csharp;
using rpc_csharp.transport;
using RPC.Transports;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Payload = Decentraland.Social.Friendships.Payload;

namespace DCL.Social.Friends
{
    public class RPCSocialApiBridge : IRPCSocialApiBridge
    {
        private const int REQUEST_TIMEOUT = 30;

        private string accessToken;

        private readonly IUserProfileBridge userProfileWebInterfaceBridge;

        public event Action<UserStatus> OnFriendAdded;
        public event Action<string> OnFriendRemoved;
        public event Action<FriendRequest> OnFriendRequestAdded;
        public event Action<string> OnFriendRequestRemoved;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestReceived;

        private ClientFriendshipsService socialClient;

        private Func<ITransport> clientTransportProvider;

        public RPCSocialApiBridge(MatrixInitializationBridge matrixInitializationBridge, IUserProfileBridge userProfileWebInterfaceBridge, Func<ITransport> transportProvider)
        {
            matrixInitializationBridge.OnReceiveMatrixAccessToken += (token) => { this.accessToken = token; };
            this.userProfileWebInterfaceBridge = userProfileWebInterfaceBridge;
            clientTransportProvider = transportProvider;
        }

        public async UniTaskVoid InitializeClient(CancellationToken cancellationToken = default)
        {
            var transport = clientTransportProvider();
            var client = new RpcClient(transport);
            var socialPort = await client.CreatePort("social-service-port");
            var module = await socialPort.LoadModule(FriendshipsServiceCodeGen.ServiceName);
            socialClient = new ClientFriendshipsService(module);

            InitializeSubscriptions(cancellationToken).Forget();
        }

        async UniTaskVoid InitializeSubscriptions(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // start listening to streams
            // await UniTask.WhenAny(this.ListenToFriendEvents(cancellationToken));
        }

        public async UniTask<FriendshipInitializationMessage> InitializeFriendshipsInformation(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await UniTask.WaitUntil(() =>
                    socialClient != null,
                PlayerLoopTiming.Update,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await WaitForAccessTokenAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var friendsStream = socialClient.GetFriends(new Payload() { SynapseToken = accessToken });

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

            await UniTask.SwitchToMainThread(cancellationToken);

            return new FriendshipInitializationMessage()
            {
                // totalReceivedRequests = requestEvents.Incoming.Items.Count,
                totalReceivedRequests = 0
            };
        }

        async UniTask ListenToFriendEvents(CancellationToken cancellationToken = default)
        {
            var ownUserProfile = userProfileWebInterfaceBridge.GetOwn();

            await WaitForAccessTokenAsync(cancellationToken);

            var stream = socialClient.SubscribeFriendshipEventsUpdates(new Payload() { SynapseToken = accessToken });

            cancellationToken.ThrowIfCancellationRequested();

            await foreach (var item in stream.WithCancellation(cancellationToken))
            {
                foreach (var friendshipEvent in item.Events)
                {
                    // var action = ToFriendshipAction(friendshipEvent.BodyCase);
                    //
                    // OnReceivedFriendshipEvent?.Invoke(new FriendshipUpdateStatusMessage()
                    // {
                    //     case FriendshipEventResponse.BodyOneofCase.None: break;
                    //     case FriendshipEventResponse.BodyOneofCase.Request: break;
                    //     case FriendshipEventResponse.BodyOneofCase.Accept: break;
                    //     case FriendshipEventResponse.BodyOneofCase.Reject: break;
                    //     case FriendshipEventResponse.BodyOneofCase.Delete: break;
                    //     case FriendshipEventResponse.BodyOneofCase.Cancel: break;
                    //     default: throw new ArgumentOutOfRangeException();
                    // }
                }
            }
        }

        private async UniTask WaitForAccessTokenAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() =>
                    !string.IsNullOrEmpty(accessToken),
                PlayerLoopTiming.Update,
                cancellationToken);
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

            return new FriendRequest(GetFriendRequestId(friendUserId, createdAt), createdAt, userProfileWebInterfaceBridge.GetOwn().userId, friendUserId, messageBody);
        }

        private static string GetUserIdFromFriendRequestId(string friendRequestId)
        {
            var firstHyphen = friendRequestId.IndexOf('-');
            return friendRequestId.Substring(0, firstHyphen);
        }

        private static string GetFriendRequestId(string userId, long createdAt) =>
            $"{userId}-{createdAt}";

        public async UniTask UpdateFriendship(UpdateFriendshipPayload updateFriendshipPayload, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                var response = await socialClient
                                    .UpdateFriendshipEvent(updateFriendshipPayload)
                                    .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT));

                cancellationToken.ThrowIfCancellationRequested();

                switch (response.ResponseCase)
                {
                    case UpdateFriendshipResponse.ResponseOneofCase.Event:
                        break;
                    case UpdateFriendshipResponse.ResponseOneofCase.Error:
                        var error = response.Error;
                        throw new FriendshipException(ToErrorCode(error));
                    case UpdateFriendshipResponse.ResponseOneofCase.None:
                    default:
                        throw new NotSupportedException(response.ResponseCase.ToString());
                }

                await UniTask.SwitchToMainThread(cancellationToken);
            }
            finally { await UniTask.SwitchToMainThread(); }
        }

        // TODO: All of the following methods will be implemented in the future, as this is an ongoing task
        // Each of them has their corresponding ticket, and won't be called since the feature flag is turned off

        public UniTask<AddFriendsPayload> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<AddFriendsPayload> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask CancelRequestByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

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

        FriendRequestErrorCodes ToErrorCode(FriendshipErrorCode code)
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
    }
}
