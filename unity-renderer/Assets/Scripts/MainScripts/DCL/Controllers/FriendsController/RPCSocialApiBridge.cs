using Cysharp.Threading.Tasks;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
using rpc_csharp;
using rpc_csharp.transport;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Payload = Decentraland.Social.Friendships.Payload;

namespace DCL.Social.Friends
{
    public class RPCSocialApiBridge : ISocialApiBridge
    {
        private const int REQUEST_TIMEOUT = 30;

        private readonly IMatrixInitializationBridge matrixInitializationBridge;
        private readonly IUserProfileBridge userProfileWebInterfaceBridge;
        private readonly Func<ITransport> clientTransportProvider;

        private string accessToken;
        private ClientFriendshipsService socialClient;
        private UniTaskCompletionSource<FriendshipInitializationMessage> initializationInformationTask;

        public event Action<UserStatus> OnFriendAdded;
        public event Action<FriendRequest> OnIncomingFriendRequestAdded;
        public event Action<FriendRequest> OnOutgoingFriendRequestAdded;
        public event Action<string> OnFriendRemoved;
        public event Action<string> OnFriendRequestRemoved;
        public event Action<string, UserProfile> OnFriendRequestAccepted;
        public event Action<string> OnFriendRequestRejected;
        public event Action<string> OnFriendRequestCanceled;
        public event Action<string> OnDeletedByFriend;

        public RPCSocialApiBridge(IMatrixInitializationBridge matrixInitializationBridge,
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
            UniTask.WhenAll(SubscribeToFriendshipEvents(cancellationToken));
        }

        public async UniTask<FriendshipInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default)
        {
            if (initializationInformationTask != null) return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);

            initializationInformationTask = new UniTaskCompletionSource<FriendshipInitializationMessage>();
            await InitializeMatrixTokenThenRetrieveAllFriends(cancellationToken);
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
            cancellationToken.ThrowIfCancellationRequested();

            await WaitForAccessTokenAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var friendsStream = socialClient.GetFriends(new Payload
                { SynapseToken = accessToken });

            List<UniTask> tasks = new List<UniTask>();

            await foreach (var friends in friendsStream.WithCancellation(cancellationToken))
            {
                tasks.AddRange(friends.Users.Users_.Select(async friend =>
                {
                    try
                    {
                        var profile = await userProfileWebInterfaceBridge.RequestFullUserProfileAsync(friend.Address, cancellationToken);
                        OnFriendAdded?.Invoke(new UserStatus { userId = friend.Address, userName = profile.userName });
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception e) { Debug.LogException(e); }
                }));
            }

            // Forgetting this since it would otherwise compromise the startup time of the explorer
            UniTask.WhenAll(tasks).Forget();

            cancellationToken.ThrowIfCancellationRequested();
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
                    },
                AuthToken = new Payload()
                {
                    SynapseToken = accessToken
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, userId, cancellationToken);
        }

        public async UniTask CancelFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default)
        {
            string userId = GetUserIdFromFriendRequestId(friendRequestId);

            var updateFriendshipPayload = new UpdateFriendshipPayload()
            {
                Event =
                    new FriendshipEventPayload()
                    {
                        Cancel = new CancelPayload()
                        {
                            User = new User() { Address = userId }
                        }
                    },
                AuthToken = new Payload()
                {
                    SynapseToken = accessToken
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, userId, cancellationToken);
        }

        public async UniTask AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default)
        {
            string userId = GetUserIdFromFriendRequestId(friendRequestId);

            var updateFriendshipPayload = new UpdateFriendshipPayload()
            {
                Event =
                    new FriendshipEventPayload()
                    {
                        Accept = new AcceptPayload()
                        {
                            User = new User() { Address = userId }
                        }
                    },
                AuthToken = new Payload()
                {
                    SynapseToken = accessToken
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, userId, cancellationToken);
        }

        public async UniTask DeleteFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload()
            {
                Event =
                    new FriendshipEventPayload()
                    {
                        Delete = new DeletePayload()
                        {
                            User = new User() { Address = friendId }
                        }
                    },
                AuthToken = new Payload()
                {
                    SynapseToken = accessToken
                }
            };

            await this.UpdateFriendship(updateFriendshipPayload, friendId, cancellationToken);
        }

        public async UniTask<FriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload()
            {
                Event = new FriendshipEventPayload()
                {
                    Request = new RequestPayload()
                    {
                        Message = messageBody,
                        User = new User() { Address = friendUserId },
                    }
                },
                AuthToken = new Payload()
                {
                    SynapseToken = accessToken
                }
            };

            return await UpdateFriendship(updateFriendshipPayload, friendUserId, cancellationToken);
        }

        private async UniTask SubscribeToFriendshipEvents(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await foreach (var friendshipEventResponse in socialClient.SubscribeFriendshipEventsUpdates(new Payload() { SynapseToken = accessToken }))
                {
                    switch (friendshipEventResponse.ResponseCase)
                    {
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.InternalServerError:
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.UnauthorizedError:
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.ForbiddenError:
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.TooManyRequestsError:
                            ProcessAndLogSubscriptionError(friendshipEventResponse);
                            break;
                        case SubscribeFriendshipEventsUpdatesResponse.ResponseOneofCase.Events:
                            await ProcessFriendshipEventResponse(friendshipEventResponse.Events, cancellationToken);
                            break;
                        default:
                        {
                            Debug.LogErrorFormat("Subscription to friendship events got invalid response {0}", friendshipEventResponse);
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.Log($"Got exception {ex.Message}"); }
        }

        private async UniTask ProcessFriendshipEventResponse(FriendshipEventResponses friendshipEventsUpdatesResponse, CancellationToken cancellationToken = default)
        {
            foreach (var friendshipEvent in friendshipEventsUpdatesResponse.Responses) { await ProcessFriendshipEvent(friendshipEvent, ProcessIncomingFriendRequestEvent, cancellationToken); }
        }

        private FriendRequest ProcessIncomingFriendRequestEvent(RequestResponse requestResponse)
        {
            var request = new FriendRequest(
                GetFriendRequestId(requestResponse.User.Address, requestResponse.CreatedAt),
                requestResponse.CreatedAt,
                requestResponse.User.Address,
                userProfileWebInterfaceBridge.GetOwn().userId,
                requestResponse.Message);

            OnIncomingFriendRequestAdded?.Invoke(request);
            return request;
        }

        private FriendRequest ProcessOutgoingFriendRequestEvent(RequestResponse requestResponse)
        {
            var request = new FriendRequest(
                GetFriendRequestId(requestResponse.User.Address, requestResponse.CreatedAt),
                requestResponse.CreatedAt,
                userProfileWebInterfaceBridge.GetOwn().userId,
                requestResponse.User.Address,
                requestResponse.Message);

            OnOutgoingFriendRequestAdded?.Invoke(request);

            return request;
        }

        private async UniTask<FriendRequest> ProcessFriendshipEvent(FriendshipEventResponse friendshipEvent, Func<RequestResponse, FriendRequest> onRequest, CancellationToken cancellationToken)
        {
            switch (friendshipEvent.BodyCase)
            {
                case FriendshipEventResponse.BodyOneofCase.Request:
                    return onRequest(friendshipEvent.Request);
                case FriendshipEventResponse.BodyOneofCase.Accept:
                    cancellationToken.ThrowIfCancellationRequested();

                    var profile =
                        await userProfileWebInterfaceBridge.RequestFullUserProfileAsync(friendshipEvent.Accept.User.Address,
                            cancellationToken);

                    OnFriendRequestAccepted?.Invoke(friendshipEvent.Accept.User.Address, profile);
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
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private void ProcessAndLogSubscriptionError(SubscribeFriendshipEventsUpdatesResponse error)
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
                default: throw new ArgumentOutOfRangeException(nameof(error), error, null);
            }
        }

        private static string GetUserIdFromFriendRequestId(string friendRequestId)
        {
            var firstHyphen = friendRequestId.IndexOf('-');
            return friendRequestId.Substring(0, firstHyphen);
        }

        private static string GetFriendRequestId(string userId, long createdAt) =>
            $"{userId}-{createdAt}";

        private async UniTask<FriendRequest> UpdateFriendship(UpdateFriendshipPayload updateFriendshipPayload, string friendId, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                var response = await socialClient
                                    .UpdateFriendshipEvent(updateFriendshipPayload)
                                    .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT));

                switch (response.ResponseCase)
                {
                    case UpdateFriendshipResponse.ResponseOneofCase.Event:
                        return await ProcessFriendshipEvent(response.Event, ProcessOutgoingFriendRequestEvent, cancellationToken);
                    case UpdateFriendshipResponse.ResponseOneofCase.InternalServerError:
                    case UpdateFriendshipResponse.ResponseOneofCase.UnauthorizedError:
                    case UpdateFriendshipResponse.ResponseOneofCase.ForbiddenError:
                    case UpdateFriendshipResponse.ResponseOneofCase.TooManyRequestsError:
                    case UpdateFriendshipResponse.ResponseOneofCase.BadRequestError:
                        ProcessAndLogUpdateFriendshipErrorError(response, friendId);
                        return null;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            finally { await UniTask.SwitchToMainThread(); }
        }

        private void ProcessAndLogUpdateFriendshipErrorError(UpdateFriendshipResponse error, string friendId)
        {
            switch (error.ResponseCase)
            {
                case UpdateFriendshipResponse.ResponseOneofCase.InternalServerError:
                    Debug.LogErrorFormat("Got internal server error while trying to update friendship {0} {1}", friendId, error.InternalServerError.Message);
                    break;
                case UpdateFriendshipResponse.ResponseOneofCase.UnauthorizedError:
                    Debug.LogErrorFormat("Got Unauthorized error while trying to update friendship {0} {1}", friendId, error.UnauthorizedError.Message);
                    break;
                case UpdateFriendshipResponse.ResponseOneofCase.ForbiddenError:
                    Debug.LogErrorFormat("Got Forbidden error while trying to update friendship {0} {1}", friendId, error.ForbiddenError.Message);
                    break;
                case UpdateFriendshipResponse.ResponseOneofCase.TooManyRequestsError:
                    Debug.LogErrorFormat("Got Too many requests error {0} {1} while trying to update friendship", friendId, error.TooManyRequestsError.Message);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(error), error, null);
            }
        }
    }
}
