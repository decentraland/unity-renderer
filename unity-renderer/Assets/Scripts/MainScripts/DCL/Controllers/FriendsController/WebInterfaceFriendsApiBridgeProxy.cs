using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;

namespace DCL.Social.Friends
{
    // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
    public class WebInterfaceFriendsApiBridgeProxy : IFriendsApiBridge
    {
        private const string NEW_FRIEND_REQUESTS_FLAG = "new_friend_requests";

        private readonly IFriendsApiBridge apiBridge;
        private readonly IFriendsApiBridge apiBridgeMock;
        private readonly DataStore dataStore;

        private bool isNewFriendRequestsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled(NEW_FRIEND_REQUESTS_FLAG);
        private IFriendsApiBridge apiBridgeInUse => isNewFriendRequestsEnabled ? apiBridgeMock : apiBridge;

        public event Action<FriendshipInitializationMessage> OnInitialized;
        public event Action<string> OnFriendNotFound;
        public event Action<AddFriendsPayload> OnFriendsAdded;
        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded;
        public event Action<UserStatus> OnUserPresenceUpdated;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated;
        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestReceived;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;

        public WebInterfaceFriendsApiBridgeProxy(IFriendsApiBridge apiBridge, IFriendsApiBridge apiBridgeMock, DataStore dataStore)
        {
            this.apiBridge = apiBridge;
            this.apiBridgeMock = apiBridgeMock;
            this.dataStore = dataStore;

            this.apiBridge.OnInitialized += x => OnInitialized?.Invoke(x);
            this.apiBridgeMock.OnInitialized += x => OnInitialized?.Invoke(x);

            this.apiBridge.OnFriendNotFound += x => OnFriendNotFound?.Invoke(x);
            this.apiBridgeMock.OnFriendNotFound += x => OnFriendNotFound?.Invoke(x);

            this.apiBridge.OnFriendsAdded += x => OnFriendsAdded?.Invoke(x);
            this.apiBridgeMock.OnFriendsAdded += x => OnFriendsAdded?.Invoke(x);

            this.apiBridge.OnFriendWithDirectMessagesAdded += x => OnFriendWithDirectMessagesAdded?.Invoke(x);
            this.apiBridgeMock.OnFriendWithDirectMessagesAdded += x => OnFriendWithDirectMessagesAdded?.Invoke(x);

            this.apiBridge.OnUserPresenceUpdated += x => OnUserPresenceUpdated?.Invoke(x);
            this.apiBridgeMock.OnUserPresenceUpdated += x => OnUserPresenceUpdated?.Invoke(x);

            this.apiBridge.OnFriendshipStatusUpdated += x => OnFriendshipStatusUpdated?.Invoke(x);
            this.apiBridgeMock.OnFriendshipStatusUpdated += x => OnFriendshipStatusUpdated?.Invoke(x);

            this.apiBridge.OnTotalFriendRequestCountUpdated += x => OnTotalFriendRequestCountUpdated?.Invoke(x);
            this.apiBridgeMock.OnTotalFriendRequestCountUpdated += x => OnTotalFriendRequestCountUpdated?.Invoke(x);

            this.apiBridge.OnTotalFriendCountUpdated += x => OnTotalFriendCountUpdated?.Invoke(x);
            this.apiBridgeMock.OnTotalFriendCountUpdated += x => OnTotalFriendCountUpdated?.Invoke(x);

            this.apiBridge.OnFriendRequestsAdded += x => OnFriendRequestsAdded?.Invoke(x);
            this.apiBridgeMock.OnFriendRequestsAdded += x => OnFriendRequestsAdded?.Invoke(x);

            this.apiBridge.OnFriendRequestReceived += x => OnFriendRequestReceived?.Invoke(x);
            this.apiBridgeMock.OnFriendRequestReceived += x => OnFriendRequestReceived?.Invoke(x);
        }

        public void RejectFriendship(string userId) =>
            apiBridgeInUse.RejectFriendship(userId);

        public UniTask<RejectFriendshipPayload> RejectFriendshipAsync(string friendRequestId) =>
            apiBridgeInUse.RejectFriendshipAsync(friendRequestId);

        public void RemoveFriend(string userId) =>
            apiBridgeInUse.RemoveFriend(userId);

        public void GetFriends(int limit, int skip) =>
            apiBridgeInUse.GetFriends(limit, skip);

        public void GetFriends(string usernameOrId, int limit) =>
            apiBridgeInUse.GetFriends(usernameOrId, limit);

        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            apiBridgeInUse.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            apiBridgeInUse.GetFriendRequestsAsync(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip) =>
            apiBridgeInUse.GetFriendsWithDirectMessages(usernameOrId, limit, skip);

        public void RequestFriendship(string friendUserId) =>
            apiBridgeInUse.RequestFriendship(friendUserId);

        public UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody) =>
            apiBridgeInUse.RequestFriendshipAsync(userId, messageBody);

        public UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string userId) =>
            apiBridgeInUse.CancelRequestAsync(userId);

        public UniTask CancelRequestByUserIdAsync(string userId) =>
            apiBridgeInUse.CancelRequestByUserIdAsync(userId);

        public void CancelRequestByUserId(string userId) =>
            apiBridgeInUse.CancelRequestByUserId(userId);

        public void AcceptFriendship(string userId) =>
            apiBridgeInUse.AcceptFriendship(userId);

        public UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId) =>
            apiBridgeInUse.AcceptFriendshipAsync(friendRequestId);
    }
}
