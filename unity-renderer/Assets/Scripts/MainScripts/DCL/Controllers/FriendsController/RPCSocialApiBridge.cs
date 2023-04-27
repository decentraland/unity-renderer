using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using Decentraland.Renderer.RendererServices;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
using RPC;
using rpc_csharp;
using rpc_csharp.transport;
using System;
using System.Threading;
using Payload = Decentraland.Social.Friendships.Payload;

namespace DCL.Social.Friends
{
    public class RPCSocialApiBridge : ISocialApiBridge
    {
        private const int REQUEST_TIMEOUT = 30;

        private readonly MatrixInitializationBridge matrixInitializationBridge;
        private readonly IUserProfileBridge userProfileWebInterfaceBridge;
        private readonly Func<ITransport> clientTransportProvider;

        private string accessToken;
        private ClientFriendshipsService socialClient;
        private UniTaskCompletionSource<FriendshipInitializationMessage> initializationInformationTask;

        public event Action<UserStatus> OnFriendAdded;
        public event Action<string> OnFriendRemoved;
        public event Action<FriendRequest> OnFriendRequestAdded;
        public event Action<string> OnFriendRequestRemoved;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestReceived;

        public RPCSocialApiBridge(MatrixInitializationBridge matrixInitializationBridge,
            IUserProfileBridge userProfileWebInterfaceBridge,
            Func<ITransport> transportProvider)
        {
            this.matrixInitializationBridge = matrixInitializationBridge;
            this.userProfileWebInterfaceBridge = userProfileWebInterfaceBridge;
            clientTransportProvider = transportProvider;
        }

        public void Dispose()
        {
            // TODO: dispose the rpc client and all related objects
        }

        public void Initialize()
        {
            matrixInitializationBridge.OnReceiveMatrixAccessToken += token => accessToken = token;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await InitializeClient(cancellationToken);
            InitializeSubscriptions(cancellationToken).Forget();
        }

        private async UniTask InitializeClient(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transport = clientTransportProvider();
            var client = new RpcClient(transport);
            var socialPort = await client.CreatePort("social-service-port");

            cancellationToken.ThrowIfCancellationRequested();

            var module = await socialPort.LoadModule(FriendshipsServiceCodeGen.ServiceName);

            cancellationToken.ThrowIfCancellationRequested();

            socialClient = new ClientFriendshipsService(module);
        }

        private async UniTaskVoid InitializeSubscriptions(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // start listening to streams
            // await UniTask.WhenAny(this.ListenToFriendEvents(cancellationToken));
        }

        public async UniTask<FriendshipInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default)
        {
            if (initializationInformationTask != null) return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);

            initializationInformationTask = new UniTaskCompletionSource<FriendshipInitializationMessage>();
            await InitializeMatrixTokenThenRetrieveAllFriends(cancellationToken);

            initializationInformationTask.TrySetResult(new FriendshipInitializationMessage
            {
                // totalReceivedRequests = requestEvents.Incoming.Items.Count,
                totalReceivedRequests = 0,
            });

            await UniTask.SwitchToMainThread(cancellationToken);

            return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);
        }

        private async UniTask InitializeMatrixTokenThenRetrieveAllFriends(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await WaitForAccessTokenAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var friendsStream = socialClient.GetFriends(new Payload
                { SynapseToken = accessToken });

            await foreach (var friends in friendsStream.WithCancellation(cancellationToken))
            {
                foreach (var friend in friends.Users_)
                    OnFriendAdded?.Invoke(new UserStatus { userId = friend.Address });
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
        }

        private async UniTask ListenToFriendEvents(CancellationToken cancellationToken = default)
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

        public async UniTask RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default)
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

        public async UniTask<FriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody, CancellationToken cancellationToken = default)
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

        private async UniTask UpdateFriendship(UpdateFriendshipPayload updateFriendshipPayload, CancellationToken cancellationToken = default)
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

        public UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string userId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<FriendshipStatus> GetFriendshipStatusAsync(string userId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public UniTask<ApproveFriendRequestReply> ApproveFriendRequestAsync(ApproveFriendRequestPayload request, RPCContext context, CancellationToken ct) =>
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
