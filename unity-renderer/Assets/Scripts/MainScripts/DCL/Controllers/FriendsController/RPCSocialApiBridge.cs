using Cysharp.Threading.Tasks;
using Decentraland.Renderer.Common;
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
        public event Action<string> OnFriendRemoved;
        public event Action<FriendRequest> OnFriendRequestAdded;
        public event Action<string> OnFriendRequestRemoved;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestReceived;

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

            // TODO: start listening to streams
        }

        public async UniTask<FriendshipInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default)
        {
            if (initializationInformationTask != null) return await initializationInformationTask.Task.AttachExternalCancellation(cancellationToken);

            initializationInformationTask = new UniTaskCompletionSource<FriendshipInitializationMessage>();
            await InitializeMatrixTokenThenRetrieveAllFriends(cancellationToken);

            initializationInformationTask.TrySetResult(new FriendshipInitializationMessage
            {
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

            List<UniTask> tasks = new List<UniTask>();

            await foreach (var friends in friendsStream.WithCancellation(cancellationToken))
            {
                tasks.AddRange(friends.Users_.Select(async friend =>
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
                Event = new FriendshipEventPayload()
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

                await UniTask.SwitchToMainThread(cancellationToken);
            }
            finally { await UniTask.SwitchToMainThread(); }
        }
    }
}
