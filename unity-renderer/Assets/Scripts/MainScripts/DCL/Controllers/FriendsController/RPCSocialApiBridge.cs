using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using DCL.Social.Friends;
using Decentraland.Echo;
using Decentraland.Renderer.RendererServices;
using Google.Protobuf.WellKnownTypes;
using NSubstitute.ReceivedExtensions;
using RPC;
using System;
using System.Threading;
using rpc_csharp;
using rpc_csharp.transport;
using System.Collections.Generic;
using Empty = Decentraland.Echo.Empty;

namespace MainScripts.DCL.Controllers.FriendsController
{
    public class RPCSocialApiBridge
    {
        private const int REQUEST_TIMEOUT = 30;

        public event Action<string> OnFriendNotFound;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded;
        public event Action<UserStatus> OnUserPresenceUpdated;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated;
        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestReceived;

        private ClientFriendshipsService clientBookService;

        private readonly Dictionary<string, FriendRequest> friendRequests = new ();
        private readonly Dictionary<string, UserStatus> friends = new ();

        public RPCSocialApiBridge()
        {
            this.friendRequests = new Dictionary<string, FriendRequest>();
            this.friends = new Dictionary<string, UserStatus>();
        }

        public async UniTaskVoid InitializeClient(ITransport transport, CancellationToken cancellationToken = default)
        {
            var client = new RpcClient(transport);

            cancellationToken.ThrowIfCancellationRequested();

            var port = await client.CreatePort("rpc-social-service");

            cancellationToken.ThrowIfCancellationRequested();

            var module = await port.LoadModule(FriendshipsServiceCodeGen.ServiceName);

            this.clientBookService = new ClientFriendshipsService(module);
        }

        public async UniTask<FriendshipInitializationMessage> GetInitializationMessage(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var friends = await clientBookService.GetFriends(new Empty());

            cancellationToken.ThrowIfCancellationRequested();

            var friendRequests = await clientBookService.GetRequestEvents(new Empty());

            foreach (var friendRequest in friendRequests.Incoming.Items) { this.friendRequests.Add(friendRequest.User.Address, new FriendRequest("GET FRIEND REQUEST ID", friendRequest.CreatedAt, friendRequest.User.Address, "GET OWN ADDRESS", friendRequest.Message)); }

            foreach (var friendRequest in friendRequests.Outgoing.Items) { this.friendRequests.Add(friendRequest.User.Address, new FriendRequest("GET FRIEND REQUEST ID", friendRequest.CreatedAt, "GET OWN ADDRESS", friendRequest.User.Address, friendRequest.Message)); }

            return new FriendshipInitializationMessage()
            {
                totalReceivedRequests = friendRequests.Incoming.Items.Count,
            };
        }

        private async UniTask ListenToFriendChanges()
        {
            // var stream = this.clientBookService.SubscribeFriendshipRequests();
            //
            // await foreach (var item in stream)
            // {
            //     // this.friends.Add("asd", new User() { Address = "asd" });
            //
            //     // this.OnAddNewFriend?.Invoke(item);
            // }
        }

        private void RemoveFriendRequest(string userId)
        {
            this.friendRequests.Remove(userId);
        }

        public async UniTask RejectFriendship(string userId, CancellationToken cancellationToken = default)
        {
            var updateFriendshipPayload = new UpdateFriendshipPayload() { Event = new Event() { Reject = new Reject() { User = new User() { Address = userId } } } };

            await this.UpdateFriendship(userId, updateFriendshipPayload, this.RemoveFriendRequest, cancellationToken);
        }

        private async UniTask UpdateFriendship(string userId, UpdateFriendshipPayload updateFriendshipPayload, Action<string> update, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                var response = await clientBookService.UpdateFriendship(updateFriendshipPayload)
                                                      .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT));

                switch (response.ReplyCase)
                {
                    case UpdateFriendshipResponse.ReplyOneofCase.Error:
                        var error = response.Error;
                        throw new FriendshipException(ToErrorCode(error));
                    case UpdateFriendshipResponse.ReplyOneofCase.Event:
                        update.Invoke(userId);
                        break;
                    case UpdateFriendshipResponse.ReplyOneofCase.None:
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

        public void RequestFriendship(string friendUserId)
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

        private static FriendRequestPayload ToFriendRequestPayload(UpdateFriendshipPayload request) =>
            new ()
            {
                to = request.Event.Request.User.Address,
                messageBody = request.Event.Request.Message,
            };

        private FriendRequestErrorCodes ToErrorCode(FriendshipErrorCode code)
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
