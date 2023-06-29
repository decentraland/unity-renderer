using Cysharp.Threading.Tasks;
using DCL.Tasks;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class RPCSocialApiBridge : ISocialApiBridge
    {
        private const int REQUEST_TIMEOUT = 30;
        private const int MAX_MINUTES_WAIT_TIME = 3;
        private const int MAX_RECONNECT_RETRIES = 3;

        private readonly IMatrixInitializationBridge matrixInitializationBridge;
        private readonly IUserProfileBridge userProfileWebInterfaceBridge;
        private readonly ISocialClientProvider socialClientProvider;

        private string accessToken;
        private IClientFriendshipsService socialClient;
        private UniTaskCompletionSource<AllFriendsInitializationMessage> initializationInformationTask;
        private CancellationTokenSource initializationCancellationToken = new ();
        private CancellationTokenSource incomingEventsSubscriptionCancellationToken = new ();

        public event Action<FriendRequest> OnIncomingFriendRequestAdded;
        public event Action<FriendRequest> OnOutgoingFriendRequestAdded;
        public event Action<string> OnFriendRequestAccepted;
        public event Action<string> OnFriendRequestRejected;
        public event Action<string> OnFriendRequestCanceled;
        public event Action<string> OnDeletedByFriend;

        private int transportFailures = 0;

        public RPCSocialApiBridge(IMatrixInitializationBridge matrixInitializationBridge,
            IUserProfileBridge userProfileWebInterfaceBridge,
            ISocialClientProvider socialClientProvider)
        {
            this.matrixInitializationBridge = matrixInitializationBridge;
            this.userProfileWebInterfaceBridge = userProfileWebInterfaceBridge;
            this.socialClientProvider = socialClientProvider;
            this.socialClientProvider.OnTransportError += OnTransportError;
        }

        public void Dispose()
        {
            initializationInformationTask?.TrySetCanceled();
            initializationCancellationToken.SafeCancelAndDispose();
            incomingEventsSubscriptionCancellationToken.SafeCancelAndDispose();
        }

        public void Initialize()
        {
            accessToken = matrixInitializationBridge.AccessToken;
            matrixInitializationBridge.OnReceiveMatrixAccessToken += token => accessToken = token;

            async UniTaskVoid InitializeAsync(CancellationToken cancellationToken)
            {
                await InitializeClient(cancellationToken);
                await WaitForAccessTokenAsync(cancellationToken);

                incomingEventsSubscriptionCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // this is an endless task that's why is forgotten
                SubscribeToIncomingFriendshipEvents(incomingEventsSubscriptionCancellationToken.Token).Forget();
            }

            initializationCancellationToken = initializationCancellationToken.SafeRestart();
            InitializeAsync(initializationCancellationToken.Token).Forget();
        }

        private async void OnTransportError()
        {
            socialClient = null;

            incomingEventsSubscriptionCancellationToken = incomingEventsSubscriptionCancellationToken.SafeRestartLinked(initializationCancellationToken.Token);

            if (transportFailures >= MAX_RECONNECT_RETRIES)
            {
                Debug.LogError("Max reconnect retries reached");
                return;
            }

            while (transportFailures < MAX_RECONNECT_RETRIES)
            {
                Debug.Log("Reconnecting to Social service");

                transportFailures++;

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(
                    TimeSpan.FromSeconds(Math.Pow(5, transportFailures) + REQUEST_TIMEOUT)
                );

                await InitializeClient(cancellationTokenSource.Token);

                if (socialClient != null)
                {
                    // this is an endless task that's why is forgotten
                    SubscribeToIncomingFriendshipEvents(incomingEventsSubscriptionCancellationToken.Token).Forget();
                    transportFailures = 0;

                    return;
                }
            }
        }

        private async UniTask InitializeClient(CancellationToken cancellationToken = default)
        {
            try { socialClient = await socialClientProvider.Provide(cancellationToken); }
            catch (Exception e) { Debug.LogException(e); }
        }

        public async UniTask<AllFriendsInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default)
        {
            if (initializationInformationTask != null) return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);

            initializationInformationTask = new UniTaskCompletionSource<AllFriendsInitializationMessage>();

            if (socialClient == null) { await WaitForSocialClient(cancellationToken); }

            await WaitForAccessTokenAsync(cancellationToken);

            // TODO: the bridge should not fetch all friends at start, its a responsibility/design issue.
            // It should be fetched by its request function accordingly
            List<string> allFriends = await GetAllFriends(cancellationToken);

            // TODO: the bridge should not fetch all friend requests at start, its a responsibility/design issue.
            // It should be fetched by its request function accordingly
            (List<FriendRequest> incoming, List<FriendRequest> outgoing) = await GetAllFriendRequests(cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);

            initializationInformationTask.TrySetResult(new AllFriendsInitializationMessage(
                allFriends, incoming, outgoing));

            return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);
        }

        private async UniTask<(List<FriendRequest> incoming, List<FriendRequest> outgoing)> GetAllFriendRequests(
            CancellationToken cancellationToken = default)
        {
            List<FriendRequest> incoming = new ();
            List<FriendRequest> outgoing = new ();

            cancellationToken.ThrowIfCancellationRequested();

            if (socialClient == null) { await WaitForSocialClient(cancellationToken); }

            var requestEvents = await socialClient.GetRequestEvents(new Payload
                { SynapseToken = accessToken });

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var friendRequest in requestEvents.Events.Incoming.Items)
            {
                incoming.Add(new FriendRequest(
                    GetFriendRequestId(friendRequest.User.Address, friendRequest.CreatedAt),

                    // timestamps comes in seconds instead of milliseconds, so do the conversion
                    DateTimeOffset.FromUnixTimeMilliseconds(friendRequest.CreatedAt * 1000L).DateTime,
                    friendRequest.User.Address,
                    userProfileWebInterfaceBridge.GetOwn().userId,
                    friendRequest.Message));
            }

            foreach (var friendRequest in requestEvents.Events.Outgoing.Items)
            {
                outgoing.Add(new FriendRequest(
                    GetFriendRequestId(friendRequest.User.Address, friendRequest.CreatedAt),

                    // timestamps comes in seconds instead of milliseconds, so do the conversion
                    DateTimeOffset.FromUnixTimeMilliseconds(friendRequest.CreatedAt * 1000L).DateTime,
                    userProfileWebInterfaceBridge.GetOwn().userId,
                    friendRequest.User.Address,
                    friendRequest.Message));
            }

            return (incoming: incoming, outgoing: outgoing);
        }

        private async UniTask<List<string>> GetAllFriends(CancellationToken cancellationToken)
        {
            List<string> result = new ();

            if (socialClient == null) { await WaitForSocialClient(cancellationToken); }

            var friendsStream = socialClient.GetFriends(new Payload
                { SynapseToken = accessToken });

            await foreach (var friends in friendsStream.WithCancellation(cancellationToken))
            {
                foreach (User friend in friends.Users.Users_)
                    result.Add(friend.Address);
            }

            return result;
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

                // timestamps comes in seconds instead of milliseconds, so do the conversion
                DateTimeOffset.FromUnixTimeMilliseconds(response.CreatedAt * 1000L).DateTime,
                userProfileWebInterfaceBridge.GetOwn().userId,
                response.User.Address,
                response.Message);
        }

        private async UniTask SubscribeToIncomingFriendshipEvents(CancellationToken cancellationToken = default)
        {
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
                            catch (OperationCanceledException) { throw; }
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

                        // timestamps comes in seconds instead of milliseconds, so do the conversion
                        DateTimeOffset.FromUnixTimeMilliseconds(response.CreatedAt * 1000L).DateTime,
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

                if (socialClient == null) { await WaitForSocialClient(cancellationToken); }

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

        private UniTask WaitForSocialClient(CancellationToken cancellationToken)
        {
            CancellationTokenSource timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCancellationTokenSource.CancelAfterSlim(TimeSpan.FromMinutes(MAX_MINUTES_WAIT_TIME));

            return UniTask.WaitUntil(() => socialClient != null, cancellationToken: timeoutCancellationTokenSource.Token);
        }
    }
}
