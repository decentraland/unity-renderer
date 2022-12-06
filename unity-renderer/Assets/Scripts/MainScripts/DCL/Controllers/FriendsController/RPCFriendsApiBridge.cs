using Cysharp.Threading.Tasks;
using DCL;
using DCL.Social.Friends;
using System;
using System.Linq;

namespace DCl.Social.Friends
{
    public class RPCFriendsApiBridge : IFriendsApiBridge
    {
        private readonly IRPC rpc;
        private readonly IFriendsApiBridge fallbackApiBridge;

        public event Action<FriendshipInitializationMessage> OnInitialized
        {
            add => fallbackApiBridge.OnInitialized += value;
            remove => fallbackApiBridge.OnInitialized -= value;
        }
        public event Action<string> OnFriendNotFound
        {
            add => fallbackApiBridge.OnFriendNotFound += value;
            remove => fallbackApiBridge.OnFriendNotFound -= value;
        }
        public event Action<AddFriendsPayload> OnFriendsAdded
        {
            add => fallbackApiBridge.OnFriendsAdded += value;
            remove => fallbackApiBridge.OnFriendsAdded -= value;
        }
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded
        {
            add => fallbackApiBridge.OnFriendRequestsAdded += value;
            remove => fallbackApiBridge.OnFriendRequestsAdded -= value;
        }
        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded
        {
            add => fallbackApiBridge.OnFriendWithDirectMessagesAdded += value;
            remove => fallbackApiBridge.OnFriendWithDirectMessagesAdded -= value;
        }
        public event Action<UserStatus> OnUserPresenceUpdated
        {
            add => fallbackApiBridge.OnUserPresenceUpdated += value;
            remove => fallbackApiBridge.OnUserPresenceUpdated -= value;
        }
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated
        {
            add => fallbackApiBridge.OnFriendshipStatusUpdated += value;
            remove => fallbackApiBridge.OnFriendshipStatusUpdated -= value;
        }
        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated
        {
            add => fallbackApiBridge.OnTotalFriendRequestCountUpdated += value;
            remove => fallbackApiBridge.OnTotalFriendRequestCountUpdated -= value;
        }
        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated
        {
            add => fallbackApiBridge.OnTotalFriendCountUpdated += value;
            remove => fallbackApiBridge.OnTotalFriendCountUpdated -= value;
        }
        public event Action<FriendRequestPayload> OnFriendRequestAdded
        {
            add => fallbackApiBridge.OnFriendRequestAdded += value;
            remove => fallbackApiBridge.OnFriendRequestAdded -= value;
        }

        public RPCFriendsApiBridge(IRPC rpc, IFriendsApiBridge fallbackApiBridge)
        {
            this.rpc = rpc;
            this.fallbackApiBridge = fallbackApiBridge;
        }

        public void RejectFriendship(string userId) =>
            fallbackApiBridge.RejectFriendship(userId);

        public void RemoveFriend(string userId) =>
            fallbackApiBridge.RemoveFriend(userId);

        public void GetFriends(int limit, int skip) =>
            fallbackApiBridge.GetFriends(limit, skip);

        public void GetFriends(string usernameOrId, int limit) =>
            fallbackApiBridge.GetFriends(usernameOrId, limit);

        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            fallbackApiBridge.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public async UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
        {
            GetFriendRequestsReply response = await rpc.FriendRequests()
                .GetFriendRequests(new GetFriendRequestsPayload
                {
                    SentLimit = sentLimit,
                    SentSkip = sentSkip,
                    ReceivedLimit = receivedLimit,
                    ReceivedSkip = receivedSkip
                });

            return new AddFriendRequestsV2Payload
            {
                requestedTo = response.Reply.RequestedTo.Select(ToFriendRequestPayload).ToArray(),
                requestedFrom = response.Reply.RequestedFrom.Select(ToFriendRequestPayload).ToArray(),
                totalReceivedFriendRequests = response.Reply.TotalReceivedFriendRequests,
                totalSentFriendRequests = response.Reply.TotalSentFriendRequests,
            };
        }

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip) =>
            fallbackApiBridge.GetFriendsWithDirectMessages(usernameOrId, limit, skip);

        public void RequestFriendship(string friendUserId) =>
            fallbackApiBridge.RequestFriendship(friendUserId);

        public UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody) =>
            fallbackApiBridge.RequestFriendshipAsync(userId, messageBody);

        UniTask<CancelFriendshipConfirmationPayload> IFriendsApiBridge.CancelRequestAsync(string friendRequestId) =>
            fallbackApiBridge.CancelRequestAsync(friendRequestId);

        public UniTask CancelRequestByUserIdAsync(string userId) =>
            fallbackApiBridge.CancelRequestByUserIdAsync(userId);

        public void CancelRequestByUserId(string userId) =>
            fallbackApiBridge.CancelRequestByUserId(userId);

        public void CancelRequest(string userId) =>
            fallbackApiBridge.CancelRequestAsync(userId);

        public void AcceptFriendship(string userId) =>
            fallbackApiBridge.AcceptFriendship(userId);

        private static FriendRequestPayload ToFriendRequestPayload(FriendRequestInfo request) =>
            new FriendRequestPayload
            {
                from = request.From,
                timestamp = (long) request.Timestamp,
                to = request.To,
                messageBody = request.MessageBody,
                friendRequestId = request.FriendRequestId
            };
    }
}
