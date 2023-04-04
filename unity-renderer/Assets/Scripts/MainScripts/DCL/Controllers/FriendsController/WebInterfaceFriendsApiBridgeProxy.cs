using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
    public class WebInterfaceFriendsApiBridgeProxy : IFriendsApiBridge
    {
        private const string NEW_FRIEND_REQUESTS_FLAG = "new_friend_requests";

        private readonly IFriendsApiBridge fallbackApiBridge;
        private readonly IFriendsApiBridge newFriendsApiBridge;
        private readonly DataStore dataStore;

        private bool isNewFriendRequestsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled(NEW_FRIEND_REQUESTS_FLAG);
        private IFriendsApiBridge apiBridgeInUse => isNewFriendRequestsEnabled ? newFriendsApiBridge : fallbackApiBridge;

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

        public WebInterfaceFriendsApiBridgeProxy(IFriendsApiBridge fallbackApiBridge, IFriendsApiBridge newFriendsApiBridge, DataStore dataStore)
        {
            this.fallbackApiBridge = fallbackApiBridge;
            this.newFriendsApiBridge = newFriendsApiBridge;
            this.dataStore = dataStore;

            this.fallbackApiBridge.OnInitialized += x => OnInitialized?.Invoke(x);
            this.newFriendsApiBridge.OnInitialized += x => OnInitialized?.Invoke(x);

            this.fallbackApiBridge.OnFriendNotFound += x => OnFriendNotFound?.Invoke(x);
            this.newFriendsApiBridge.OnFriendNotFound += x => OnFriendNotFound?.Invoke(x);

            this.fallbackApiBridge.OnFriendWithDirectMessagesAdded += x => OnFriendWithDirectMessagesAdded?.Invoke(x);
            this.newFriendsApiBridge.OnFriendWithDirectMessagesAdded += x => OnFriendWithDirectMessagesAdded?.Invoke(x);

            this.fallbackApiBridge.OnUserPresenceUpdated += x => OnUserPresenceUpdated?.Invoke(x);
            this.newFriendsApiBridge.OnUserPresenceUpdated += x => OnUserPresenceUpdated?.Invoke(x);

            this.fallbackApiBridge.OnFriendshipStatusUpdated += x => OnFriendshipStatusUpdated?.Invoke(x);
            this.newFriendsApiBridge.OnFriendshipStatusUpdated += x => OnFriendshipStatusUpdated?.Invoke(x);

            this.fallbackApiBridge.OnTotalFriendRequestCountUpdated += x => OnTotalFriendRequestCountUpdated?.Invoke(x);
            this.newFriendsApiBridge.OnTotalFriendRequestCountUpdated += x => OnTotalFriendRequestCountUpdated?.Invoke(x);

            this.fallbackApiBridge.OnTotalFriendCountUpdated += x => OnTotalFriendCountUpdated?.Invoke(x);
            this.newFriendsApiBridge.OnTotalFriendCountUpdated += x => OnTotalFriendCountUpdated?.Invoke(x);

            this.fallbackApiBridge.OnFriendRequestsAdded += x => OnFriendRequestsAdded?.Invoke(x);
            this.newFriendsApiBridge.OnFriendRequestsAdded += x => OnFriendRequestsAdded?.Invoke(x);

            this.fallbackApiBridge.OnFriendRequestReceived += x => OnFriendRequestReceived?.Invoke(x);
            this.newFriendsApiBridge.OnFriendRequestReceived += x => OnFriendRequestReceived?.Invoke(x);
        }

        public void RejectFriendship(string userId) =>
            apiBridgeInUse.RejectFriendship(userId);

        public UniTask<RejectFriendshipPayload> RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken) =>
            apiBridgeInUse.RejectFriendshipAsync(friendRequestId, cancellationToken);

        public void RemoveFriend(string userId) =>
            apiBridgeInUse.RemoveFriend(userId);

        public UniTask<AddFriendsPayload> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default) =>
            apiBridgeInUse.GetFriendsAsync(limit, skip, cancellationToken);

        public UniTask<AddFriendsPayload> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default) =>
            apiBridgeInUse.GetFriendsAsync(usernameOrId, limit, cancellationToken);

        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            apiBridgeInUse.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip,
            int receivedLimit, int receivedSkip,
            CancellationToken cancellationToken) =>
            apiBridgeInUse.GetFriendRequestsAsync(sentLimit, sentSkip, receivedLimit, receivedSkip, cancellationToken);

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip) =>
            apiBridgeInUse.GetFriendsWithDirectMessages(usernameOrId, limit, skip);

        public void RequestFriendship(string friendUserId) =>
            apiBridgeInUse.RequestFriendship(friendUserId);

        public UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody,
            CancellationToken cancellationToken) =>
            apiBridgeInUse.RequestFriendshipAsync(userId, messageBody, cancellationToken);

        public UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string userId, CancellationToken cancellationToken) =>
            apiBridgeInUse.CancelRequestAsync(userId, cancellationToken);

        public UniTask CancelRequestByUserIdAsync(string userId, CancellationToken cancellationToken) =>
            apiBridgeInUse.CancelRequestByUserIdAsync(userId, cancellationToken);

        public void CancelRequestByUserId(string userId) =>
            apiBridgeInUse.CancelRequestByUserId(userId);

        public void AcceptFriendship(string userId) =>
            apiBridgeInUse.AcceptFriendship(userId);

        public UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken) =>
            apiBridgeInUse.AcceptFriendshipAsync(friendRequestId, cancellationToken);

        public UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken) =>
            newFriendsApiBridge.GetFriendshipStatus(userId, cancellationToken);
    }
}
