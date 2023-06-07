using Cysharp.Threading.Tasks;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
using rpc_csharp;
using rpc_csharp.transport;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class RPCSocialApiBridge : ISocialApiBridge
    {
        private const int REQUEST_TIMEOUT = 30;

        private readonly IMatrixInitializationBridge matrixInitializationBridge;
        private readonly IUserProfileBridge userProfileWebInterfaceBridge;
        private readonly Func<CancellationToken, UniTask<ITransport>> clientTransportProvider;

        private string accessToken;
        private ClientFriendshipsService socialClient;
        private UniTaskCompletionSource<FriendshipInitializationMessage> initializationInformationTask;

        public event Action<string> OnFriendAdded;
        public event Action<FriendRequest> OnIncomingFriendRequestAdded;
        public event Action<FriendRequest> OnOutgoingFriendRequestAdded;
        public event Action<string> OnFriendRequestRemoved;
        public event Action<string> OnFriendRequestAccepted;
        public event Action<string> OnFriendRequestRejected;
        public event Action<string> OnFriendRequestCanceled;
        public event Action<string> OnDeletedByFriend;

        public RPCSocialApiBridge(IMatrixInitializationBridge matrixInitializationBridge,
            IUserProfileBridge userProfileWebInterfaceBridge,
            Func<CancellationToken, UniTask<ITransport>> transportProvider)
        {
            this.matrixInitializationBridge = matrixInitializationBridge;
            this.userProfileWebInterfaceBridge = userProfileWebInterfaceBridge;
            clientTransportProvider = transportProvider;
        }

        public void Dispose()
        {
            initializationInformationTask?.TrySetCanceled();
        }

        public void Initialize()
        {
            matrixInitializationBridge.OnReceiveMatrixAccessToken += token => accessToken = token;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await InitializeClient(cancellationToken);
            // start listening to streams
            UniTask.WhenAll(SubscribeToIncomingFriendshipEvents(cancellationToken)).Forget();
        }

        private async UniTask InitializeClient(CancellationToken cancellationToken = default)
        {
            var transport = await clientTransportProvider(cancellationToken);
            var client = new RpcClient(transport);
            var socialPort = await client.CreatePort("social-service-port");

            cancellationToken.ThrowIfCancellationRequested();

            var module = await socialPort.LoadModule(FriendshipsServiceCodeGen.ServiceName);

            cancellationToken.ThrowIfCancellationRequested();

            socialClient = new ClientFriendshipsService(module);
        }

        public async UniTask<FriendshipInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default)
        {
            if (initializationInformationTask != null) return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);

            initializationInformationTask = new UniTaskCompletionSource<FriendshipInitializationMessage>();

            // TODO: the bridge should not fetch all friends at start, its a responsibility/design issue.
            // It should be fetched by its request function accordingly
            await InitializeMatrixTokenThenRetrieveAllFriends(cancellationToken);

            // TODO: the bridge should not fetch all friend requests at start, its a responsibility/design issue.
            // It should be fetched by its request function accordingly
            var friendshipInitializationMessage = await GetFriendRequestsFromServer(cancellationToken);

            initializationInformationTask.TrySetResult(friendshipInitializationMessage);

            await UniTask.SwitchToMainThread(cancellationToken);

            return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);
        }

        private async UniTask<FriendshipInitializationMessage> GetFriendRequestsFromServer(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestEvents = await socialClient.GetRequestEvents(new Payload
                { SynapseToken = accessToken });

            cancellationToken.ThrowIfCancellationRequested();

            await UniTask.SwitchToMainThread(cancellationToken);

            foreach (var friendRequest in requestEvents.Events.Incoming.Items)
            {
                OnIncomingFriendRequestAdded?.Invoke(new FriendRequest(
                    GetFriendRequestId(friendRequest.User.Address, friendRequest.CreatedAt),
                    friendRequest.CreatedAt,
                    friendRequest.User.Address,
                    userProfileWebInterfaceBridge.GetOwn().userId,
                    friendRequest.Message));
            }

            foreach (var friendRequest in requestEvents.Events.Outgoing.Items)
            {
                OnOutgoingFriendRequestAdded?.Invoke(new FriendRequest(
                    GetFriendRequestId(friendRequest.User.Address, friendRequest.CreatedAt),
                    friendRequest.CreatedAt,
                    userProfileWebInterfaceBridge.GetOwn().userId,
                    friendRequest.User.Address,
                    friendRequest.Message));
            }

            return new FriendshipInitializationMessage()
            {
                totalReceivedRequests = requestEvents.Events.Incoming.Items.Count
            };
        }

        private async UniTask InitializeMatrixTokenThenRetrieveAllFriends(CancellationToken cancellationToken)
        {
            await WaitForAccessTokenAsync(cancellationToken);

            var friendsStream = socialClient.GetFriends(new Payload
                { SynapseToken = accessToken });

            await foreach (var friends in friendsStream.WithCancellation(cancellationToken))
            {
                await UniTask.SwitchToMainThread(cancellationToken);

                foreach (User friend in friends.Users.Users_)
                    OnFriendAdded?.Invoke(friend.Address);
            }
        }

        private async UniTask WaitForAccessTokenAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() =>
                    !string.IsNullOrEmpty(accessToken),
                PlayerLoopTiming.Update,
                cancellationToken);
        }

        public async UniTask RejectFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload
            {
                Event =
                    new FriendshipEventPayload
                    {
                        Reject = new RejectPayload
                        {
                            User = new User
                                { Address = friendId },
                        }
                    },
                AuthToken = new Payload
                {
                    SynapseToken = accessToken,
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, friendId, cancellationToken);
        }

        public async UniTask CancelFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload
            {
                Event =
                    new FriendshipEventPayload
                    {
                        Cancel = new CancelPayload
                        {
                            User = new User
                                { Address = friendId }
                        }
                    },
                AuthToken = new Payload
                {
                    SynapseToken = accessToken,
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, friendId, cancellationToken);
        }

        public async UniTask AcceptFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload
            {
                Event =
                    new FriendshipEventPayload
                    {
                        Accept = new AcceptPayload
                        {
                            User = new User
                                { Address = friendId }
                        }
                    },
                AuthToken = new Payload
                {
                    SynapseToken = accessToken,
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, friendId, cancellationToken);
        }

        public async UniTask DeleteFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload
            {
                Event =
                    new FriendshipEventPayload
                    {
                        Delete = new DeletePayload
                        {
                            User = new User
                                { Address = friendId }
                        }
                    },
                AuthToken = new Payload
                {
                    SynapseToken = accessToken,
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, friendId, cancellationToken);
        }

        public async UniTask<FriendRequest> RequestFriendshipAsync(string friendId, string messageBody, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload
            {
                Event = new FriendshipEventPayload
                {
                    Request = new RequestPayload
                    {
                        Message = messageBody,
                        User = new User
                            { Address = friendId },
                    }
                },
                AuthToken = new Payload
                {
                    SynapseToken = accessToken,
                }
            };

            FriendshipEventResponse @event = await UpdateFriendship(updateFriendshipPayload, friendId, cancellationToken);
            RequestResponse response = @event.Request;

            return new FriendRequest(
                GetFriendRequestId(response.User.Address, response.CreatedAt),
                response.CreatedAt,
                userProfileWebInterfaceBridge.GetOwn().userId,
                response.User.Address,
                response.Message);
        }

        private async UniTask SubscribeToIncomingFriendshipEvents(CancellationToken cancellationToken = default)
        {
            await WaitForAccessTokenAsync(cancellationToken);

            try
            {
                IUniTaskAsyncEnumerable<SubscribeFriendshipEventsUpdatesResponse> stream = socialClient
                   .SubscribeFriendshipEventsUpdates(new Payload { SynapseToken = accessToken });

                await foreach (var friendshipEventResponse in stream.WithCancellation(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    switch (friendshipEventResponse.ResponseCase)
                    {
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.InternalServerError:
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.UnauthorizedError:
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.ForbiddenError:
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.TooManyRequestsError:
                            LogIncomingFriendshipUpdateEventError(friendshipEventResponse);
                            break;
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.Events:
                            try { await ProcessIncomingFriendshipEvent(friendshipEventResponse.Events, cancellationToken); }
                            catch (OperationCanceledException) { }
                            catch (Exception e)
                            {
                                // just log the exception so we keep receiving updates in the loop
                                Debug.LogException(e);
                            }
                            break;
                        default:
                        {
                            Debug.LogErrorFormat("Subscription to friendship events got invalid response {0}", friendshipEventResponse.ResponseCase);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        private async UniTask ProcessIncomingFriendshipEvent(FriendshipEventResponses friendshipEventsUpdatesResponse,
            CancellationToken cancellationToken = default)
        {
            foreach (var friendshipEvent in friendshipEventsUpdatesResponse.Responses)
                await ProcessIncomingFriendshipEvent(friendshipEvent, cancellationToken);
        }

        private async UniTask ProcessIncomingFriendshipEvent(FriendshipEventResponse friendshipEvent, CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread(cancellationToken);

            switch (friendshipEvent.BodyCase)
            {
                case FriendshipEventResponse.BodyOneofCase.Request:
                    var response = friendshipEvent.Request;

                    var request = new FriendRequest(
                        GetFriendRequestId(response.User.Address, response.CreatedAt),
                        response.CreatedAt,
                        response.User.Address,
                        userProfileWebInterfaceBridge.GetOwn().userId,
                        response.Message);

                    OnIncomingFriendRequestAdded?.Invoke(request);
                    break;
                case FriendshipEventResponse.BodyOneofCase.Accept:
                    cancellationToken.ThrowIfCancellationRequested();

                    OnFriendRequestAccepted?.Invoke(friendshipEvent.Accept.User.Address);
                    break;
                case FriendshipEventResponse.BodyOneofCase.Reject:
                    OnFriendRequestRejected?.Invoke(friendshipEvent.Reject.User.Address);
                    break;
                case FriendshipEventResponse.BodyOneofCase.Delete:
                    OnDeletedByFriend?.Invoke(friendshipEvent.Delete.User.Address);
                    break;
                case FriendshipEventResponse.BodyOneofCase.Cancel:
                    OnFriendRequestCanceled?.Invoke(friendshipEvent.Cancel.User.Address);
                    break;
                default:
                    Debug.LogErrorFormat("Invalid friendship event {0}", friendshipEvent);
                    break;
            }
        }

        private void LogIncomingFriendshipUpdateEventError(SubscribeFriendshipEventsUpdatesResponse error)
        {
            switch (error.ResponseCase)
            {
                case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.InternalServerError:
                    Debug.LogErrorFormat("Subscription to friendship events got internal server error {0}", error.InternalServerError.Message);
                    break;
                case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.UnauthorizedError:
                    Debug.LogErrorFormat("Subscription to friendship events got Unauthorized error {0}", error.UnauthorizedError.Message);
                    break;
                case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.ForbiddenError:
                    Debug.LogErrorFormat("Subscription to friendship events got Forbidden error {0}", error.ForbiddenError.Message);
                    break;
                case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.TooManyRequestsError:
                    Debug.LogErrorFormat("Subscription to friendship events got Too many requests error {0}", error.TooManyRequestsError.Message);
                    break;
            }
        }

        private static string GetFriendRequestId(string userId, long createdAt) =>
            $"{userId}-{createdAt}";

        private async UniTask<FriendshipEventResponse> UpdateFriendship(
            UpdateFriendshipPayload updateFriendshipPayload,
            string friendId,
            CancellationToken cancellationToken = default)
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
                        return response.Event;
                    case UpdateFriendshipResponse.ResponseOneofCase.InternalServerError:
                    case UpdateFriendshipResponse.ResponseOneofCase.UnauthorizedError:
                    case UpdateFriendshipResponse.ResponseOneofCase.ForbiddenError:
                    case UpdateFriendshipResponse.ResponseOneofCase.TooManyRequestsError:
                    case UpdateFriendshipResponse.ResponseOneofCase.BadRequestError:
                    default:
                        throw new Exception(GetFriendshipUpdateErrorMessage(response, friendId));
                }
            }
            finally { await UniTask.SwitchToMainThread(); }
        }

        private string GetFriendshipUpdateErrorMessage(UpdateFriendshipResponse error, string friendId)
        {
            switch (error.ResponseCase)
            {
                case UpdateFriendshipResponse.ResponseOneofCase.InternalServerError:
                    return $"Got internal server error while trying to update friendship {friendId} {error.InternalServerError.Message}";
                case UpdateFriendshipResponse.ResponseOneofCase.UnauthorizedError:
                    return $"Got Unauthorized error while trying to update friendship {friendId} {error.UnauthorizedError.Message}";
                case UpdateFriendshipResponse.ResponseOneofCase.ForbiddenError:
                    return $"Got Forbidden error while trying to update friendship {friendId} {error.ForbiddenError.Message}";
                case UpdateFriendshipResponse.ResponseOneofCase.TooManyRequestsError:
                    return $"Got Too many requests error {friendId} {error.TooManyRequestsError.Message} while trying to update friendship";
                case UpdateFriendshipResponse.ResponseOneofCase.BadRequestError:
                    return $"Got Bad request {friendId} {error.BadRequestError.Message} while trying to update friendship";
                default:
                    return $"Unsupported friendship error {error.ResponseCase}";
            }
        }
    }
}
