using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class FriendsController : IFriendsController
    {
        private const string USE_SOCIAL_CLIENT_FEATURE_FLAG = "use-social-client";
        private static readonly Comparer<long> DESCENDING_LONG_COMPARER = Comparer<long>.Create((l, l1) => -l.CompareTo(l1));

        private readonly IFriendsApiBridge apiBridge;
        private readonly ISocialApiBridge socialApiBridge;

        // Used only when social client feature flag is false
        private readonly Dictionary<string, FriendRequest> friendRequests = new ();
        private readonly Dictionary<string, FriendRequest> incomingFriendRequestsById = new ();
        private readonly Dictionary<string, FriendRequest> outgoingFriendRequestsById = new ();
        private readonly SortedList<long, FriendRequest> incomingFriendRequestsByTimestamp = new (DESCENDING_LONG_COMPARER);
        private readonly SortedList<long, FriendRequest> outgoingFriendRequestsByTimestamp = new (DESCENDING_LONG_COMPARER);
        private readonly Dictionary<string, UserStatus> friends = new ();
        private readonly SortedList<string, UserStatus> friendsSortedByName = new ();
        private readonly DataStore dataStore;

        private CancellationTokenSource controllerCancellationTokenSource = new ();
        private UniTaskCompletionSource featureFlagsInitializedTask;
        private int totalFriendCount;
        private int totalReceivedFriendRequestCount;
        private int totalSentFriendRequestCount;

        private FeatureFlag featureFlags => dataStore.featureFlags.flags.Get();

        private bool useSocialApiBridge => featureFlags.IsFeatureEnabled(USE_SOCIAL_CLIENT_FEATURE_FLAG);

        public int AllocatedFriendCount => friends.Count(f => f.Value.friendshipStatus == FriendshipStatus.FRIEND);
        public bool IsInitialized { get; private set; }

        public int ReceivedRequestCount => friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);

        public int TotalFriendCount
        {
            get => useSocialApiBridge ? friends.Count : totalFriendCount;
            private set => totalFriendCount = value;
        }

        public int TotalFriendRequestCount => TotalReceivedFriendRequestCount + TotalSentFriendRequestCount;

        public int TotalReceivedFriendRequestCount
        {
            get => useSocialApiBridge ? incomingFriendRequestsById.Count : totalReceivedFriendRequestCount;
            private set => totalReceivedFriendRequestCount = value;
        }

        public int TotalSentFriendRequestCount
        {
            get => useSocialApiBridge ? outgoingFriendRequestsById.Count : totalSentFriendRequestCount;
            private set => totalSentFriendRequestCount = value;
        }

        public int TotalFriendsWithDirectMessagesCount { get; private set; }

        public event Action<string, UserStatus> OnUpdateUserStatus;
        public event Action<string, FriendshipAction> OnUpdateFriendship;
        public event Action<string> OnFriendNotFound;
        public event Action OnInitialized;
        public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
        public event Action<int, int> OnTotalFriendRequestUpdated;
        public event Action<FriendRequest> OnFriendRequestReceived;
        public event Action<FriendRequest> OnSentFriendRequestApproved;

        public FriendsController(IFriendsApiBridge apiBridge, ISocialApiBridge socialApiBridge, DataStore dataStore)
        {
            this.apiBridge = apiBridge;
            this.socialApiBridge = socialApiBridge;
            this.dataStore = dataStore;
        }

        public void Initialize()
        {
            controllerCancellationTokenSource = controllerCancellationTokenSource.SafeRestart();

            // TODO: wrap this events into socialApiBridge since it wont be supported by the social service
            apiBridge.OnFriendNotFound += FriendNotFound;
            apiBridge.OnFriendWithDirectMessagesAdded += AddFriendsWithDirectMessages;
            apiBridge.OnUserPresenceUpdated += UpdateUserPresence;
            apiBridge.OnFriendshipStatusUpdated += HandleUpdateFriendshipStatus;

            void SubscribeToCorrespondingBridgeEvents()
            {
                if (useSocialApiBridge)
                {
                    socialApiBridge.OnFriendAdded += AddFriend;
                    socialApiBridge.OnIncomingFriendRequestAdded += AddIncomingFriendRequest;
                    socialApiBridge.OnOutgoingFriendRequestAdded += AddOutgoingFriendRequest;
                    socialApiBridge.OnFriendRemoved += InternalRemoveFriend;
                    socialApiBridge.OnFriendRequestAdded += AddFriendRequest;
                    socialApiBridge.OnFriendRequestRemoved += RemoveFriendRequest;
                }
                else
                {
                    apiBridge.OnInitialized += InitializeFriendships;

                    // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
                    apiBridge.OnFriendRequestsAdded += AddFriendRequests;
                    apiBridge.OnFriendRequestReceived += ReceiveFriendRequest;
                    apiBridge.OnTotalFriendRequestCountUpdated += UpdateTotalFriendRequests;
                    apiBridge.OnTotalFriendCountUpdated += UpdateTotalFriends;
                }
            }

            if (featureFlags.IsInitialized)
                SubscribeToCorrespondingBridgeEvents();
            else
                WaitForFeatureFlagsToBeInitialized(controllerCancellationTokenSource.Token)
                   .ContinueWith(SubscribeToCorrespondingBridgeEvents)
                   .Forget();
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (!featureFlags.IsInitialized)
                await WaitForFeatureFlagsToBeInitialized(cancellationToken);

            if (useSocialApiBridge)
            {
                FriendshipInitializationMessage info = await socialApiBridge.GetInitializationInformationAsync(cancellationToken);
                InitializeFriendships(info);
            }
        }

        public void Dispose()
        {
            controllerCancellationTokenSource.SafeCancelAndDispose();

            socialApiBridge.OnFriendAdded -= AddFriend;
            socialApiBridge.OnFriendRemoved -= InternalRemoveFriend;
            socialApiBridge.OnFriendRequestAdded -= AddFriendRequest;
            socialApiBridge.OnFriendRequestRemoved -= RemoveFriendRequest;
            socialApiBridge.OnIncomingFriendRequestAdded -= AddIncomingFriendRequest;
            socialApiBridge.OnOutgoingFriendRequestAdded -= AddOutgoingFriendRequest;

            apiBridge.OnInitialized -= InitializeFriendships;

            // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
            apiBridge.OnFriendRequestsAdded -= AddFriendRequests;
            apiBridge.OnFriendRequestReceived -= ReceiveFriendRequest;
            apiBridge.OnFriendNotFound -= FriendNotFound;
            apiBridge.OnFriendWithDirectMessagesAdded -= AddFriendsWithDirectMessages;
            apiBridge.OnUserPresenceUpdated -= UpdateUserPresence;
            apiBridge.OnFriendshipStatusUpdated -= HandleUpdateFriendshipStatus;
            apiBridge.OnTotalFriendRequestCountUpdated -= UpdateTotalFriendRequests;
            apiBridge.OnTotalFriendCountUpdated -= UpdateTotalFriends;
        }

        private void RemoveFriendRequest(string userId)
        {
            // TODO: FIX
            this.friendRequests.Remove(userId);
        }

        // TODO (Joni): Replace by AddFriendRequests, this is just for successful compilation
        private void AddFriendRequest(FriendRequest friendRequest)
        {
            // TODO: FIX
            this.friendRequests[friendRequest.FriendRequestId] = friendRequest;
        }

        private void AddFriend(UserStatus friend)
        {
            this.friends[friend.userId] = friend;
            this.friendsSortedByName[friend.userName] = friend;
        }

        private void InternalRemoveFriend(string userId)
        {
            if (!this.friends.ContainsKey(userId))
            {
                Debug.LogWarning($"Tried to remove non existing friend {userId}");
                return;
            }

            var friend = this.friends[userId];
            this.friends.Remove(userId);
            this.friendsSortedByName.Remove(friend.userName);
        }

        private void InitializeFriendships(FriendshipInitializationMessage msg)
        {
            if (IsInitialized) return;

            IsInitialized = true;

            TotalReceivedFriendRequestCount = msg.totalReceivedRequests;
            OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);
            OnInitialized?.Invoke();
        }

        public UserStatus GetUserStatus(string userId)
        {
            if (!friends.ContainsKey(userId))
                return new UserStatus { userId = userId, friendshipStatus = FriendshipStatus.NOT_FRIEND };

            return friends[userId];
        }

        public bool ContainsStatus(string friendId, FriendshipStatus status)
        {
            if (!friends.ContainsKey(friendId)) return false;
            return friends[friendId].friendshipStatus == status;
        }

        public async UniTask<FriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FriendRequest friendRequest;

            if (useSocialApiBridge)
            {
                friendRequest = await socialApiBridge.RequestFriendshipAsync(friendUserId, messageBody, cancellationToken);
                outgoingFriendRequestsById[friendRequest.FriendRequestId] = friendRequest;

                // it's impossible to have duplicated timestamps for two different correctly created friend requests
                outgoingFriendRequestsByTimestamp[friendRequest.Timestamp] = friendRequest;
            }
            else
            {
                RequestFriendshipConfirmationPayload payload = await apiBridge.RequestFriendshipAsync(friendUserId, messageBody, cancellationToken);

                FriendRequestPayload friendRequestPayload = payload.friendRequest;

                friendRequest = new (friendRequestPayload.friendRequestId,
                    friendRequestPayload.timestamp,
                    friendRequestPayload.from,
                    friendRequestPayload.to,
                    friendRequestPayload.messageBody);

                friendRequests[friendRequest.FriendRequestId] = friendRequest;
            }

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.REQUESTED_TO, userId = friendRequest.To });

            return friendRequest;
        }

        public void RequestFriendship(string friendUserId) =>
            apiBridge.RequestFriendship(friendUserId);

        public Dictionary<string, UserStatus> GetAllocatedFriends() =>
            new (friends);

        public async UniTask<FriendRequest> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AcceptFriendshipPayload payload = await apiBridge.AcceptFriendshipAsync(friendRequestId, cancellationToken);

            FriendRequestPayload requestPayload = payload.FriendRequest;
            var request = ToFriendRequest(requestPayload);
            friendRequests.Remove(request.FriendRequestId);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.APPROVED, userId = request.From });

            return request;
        }

        public void RejectFriendship(string friendUserId) =>
            apiBridge.RejectFriendship(friendUserId);

        public async UniTask<FriendRequest> RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FriendRequest friendRequest;

            if (useSocialApiBridge)
            {
                friendRequest = incomingFriendRequestsById[friendRequestId];

                await socialApiBridge.RejectFriendshipAsync(friendRequestId, cancellationToken);
            }
            else
            {
                RejectFriendshipPayload payload = await apiBridge.RejectFriendshipAsync(friendRequestId, cancellationToken);

                FriendRequestPayload requestPayload = payload.FriendRequestPayload;
                friendRequest = ToFriendRequest(requestPayload);
                friendRequests.Remove(friendRequest.FriendRequestId);
            }

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.REJECTED, userId = friendRequest.From });

            return friendRequest;
        }

        public bool IsFriend(string userId) =>
            friends.ContainsKey(userId) && friends[userId].friendshipStatus == FriendshipStatus.FRIEND;

        public void RemoveFriend(string friendId)
        {
            if (useSocialApiBridge)
            {
                // TODO: try await Call for social api bridge
                InternalRemoveFriend(friendId);
                return;
            }

            apiBridge.RemoveFriend(friendId);
        }

        public async UniTask<string[]> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
            {
                int startIndex = skip;
                int endIndex = Math.Min(skip + limit, friendsSortedByName.Count) - 1;
                int arraySize = endIndex - startIndex + 1;

                var friendIds = new string[arraySize];

                for (int i = startIndex; i <= endIndex; i++)
                {
                    string key = friendsSortedByName.Keys[i];
                    friendIds[i - startIndex] = friendsSortedByName[key].userId;
                }

                return friendIds;
            }

            var payload = await apiBridge.GetFriendsAsync(limit, skip, cancellationToken);
            await UniTask.SwitchToMainThread();

            TotalFriendCount = payload.totalFriends;
            AddFriends(payload.friends);

            return payload.friends;
        }

        public async UniTask<IReadOnlyList<string>> GetFriendsAsync(string userNameOrId, int limit, CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
            {
                var friendsToReturn = new List<string>();

                foreach (var friend in friendsSortedByName.Values)
                {
                    if (friend.userName.IndexOf(userNameOrId, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        friend.userId.IndexOf(userNameOrId, StringComparison.OrdinalIgnoreCase) >= 0) { friendsToReturn.Add(friend.userId); }

                    if (friendsToReturn.Count >= limit) { break; }
                }

                return friendsToReturn;
            }

            var payload = await apiBridge.GetFriendsAsync(userNameOrId, limit, cancellationToken);
            await UniTask.SwitchToMainThread();

            TotalFriendCount = payload.totalFriends;
            AddFriends(payload.friends);

            return payload.friends;
        }

        // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            apiBridge.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public async UniTask<IReadOnlyList<FriendRequest>> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (useSocialApiBridge)
            {
                int sentCount = outgoingFriendRequestsByTimestamp.Values.Count;
                int receivedCount = incomingFriendRequestsByTimestamp.Values.Count;

                List<FriendRequest> result = new List<FriendRequest>(sentCount - sentSkip + receivedCount - receivedSkip);

                for (int i = receivedSkip; i < receivedSkip + receivedLimit && i < incomingFriendRequestsByTimestamp.Values.Count; i++)
                {
                    var receivedRequest = incomingFriendRequestsByTimestamp.Values[i];
                    result.Add(receivedRequest);
                }

                for (int i = sentSkip; i < sentSkip + sentLimit && i < outgoingFriendRequestsByTimestamp.Values.Count; i++)
                {
                    var sentRequest = outgoingFriendRequestsByTimestamp.Values[i];
                    result.Add(sentRequest);
                }

                return result;
            }

            var payload = await apiBridge.GetFriendRequestsAsync(sentLimit, sentSkip, receivedLimit, receivedSkip, cancellationToken);

            TotalReceivedFriendRequestCount = payload.totalReceivedFriendRequests;
            TotalSentFriendRequestCount = payload.totalSentFriendRequests;
            OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);

            List<FriendRequest> receivedFriendRequestsToAdd = new List<FriendRequest>();

            foreach (var friendRequest in payload.requestedFrom)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var request = ToFriendRequest(friendRequest);
                friendRequests[request.FriendRequestId] = request;
                receivedFriendRequestsToAdd.Add(request);

                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.REQUESTED_FROM, userId = friendRequest.from });
            }

            foreach (var friendRequest in payload.requestedTo)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var request = ToFriendRequest(friendRequest);
                friendRequests[request.FriendRequestId] = request;
                receivedFriendRequestsToAdd.Add(request);

                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.REQUESTED_TO, userId = friendRequest.to });
            }

            return receivedFriendRequestsToAdd;
        }

        public void GetFriendsWithDirectMessages(int limit, int skip) =>
            apiBridge.GetFriendsWithDirectMessages("", limit, skip);

        public void GetFriendsWithDirectMessages(string userNameOrId, int limit) =>
            apiBridge.GetFriendsWithDirectMessages(userNameOrId, limit, 0);

        public bool TryGetAllocatedFriendRequest(string friendRequestId, out FriendRequest result)
        {
            if (!useSocialApiBridge) { return friendRequests.TryGetValue(friendRequestId, out result); }

            return incomingFriendRequestsById.TryGetValue(friendRequestId, out result) || outgoingFriendRequestsById.TryGetValue(friendRequestId, out result);
        }

        public FriendRequest GetAllocatedFriendRequestByUser(string userId)
        {
            long max = long.MinValue;
            FriendRequest result = null;

            if (useSocialApiBridge)
            {
                foreach (var request in incomingFriendRequestsById.Values)
                {
                    if (request.From != userId && request.To != userId) continue;
                    if (request.Timestamp <= max) continue;
                    result = request;
                    max = request.Timestamp;
                    break;
                }

                foreach (var request in outgoingFriendRequestsById.Values)
                {
                    if (request.From != userId && request.To != userId) continue;

                    // we do check for max here because within a single session there can be a
                    // sent and a received request with the same user and we want to show the latest one
                    if (request.Timestamp <= max) continue;
                    return request;
                }
            }
            else
            {
                foreach (var request in friendRequests.Values)
                {
                    if (request.From != userId && request.To != userId) continue;
                    if (request.Timestamp <= max) continue;
                    result = request;
                    max = request.Timestamp;
                }
            }

            return result;
        }

        public async UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken)
        {
            FriendshipStatus status = await apiBridge.GetFriendshipStatus(userId, cancellationToken);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
            {
                action = ToFriendshipAction(status),
                userId = userId,
            });

            return status;
        }

        public async UniTask<FriendRequest> CancelRequestByUserIdAsync(string friendUserId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FriendRequest request = GetAllocatedFriendRequestByUser(friendUserId);

            if (request != null)
            {
                await apiBridge.CancelRequestAsync(request.FriendRequestId, cancellationToken);

                friendRequests.Remove(request.FriendRequestId);

                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.CANCELLED, userId = friendUserId });

                return request;
            }

            // TODO (FRIEND REQUESTS): this operation is deprecated and should be removed after the release of improved friend requests
            await apiBridge.CancelRequestByUserIdAsync(friendUserId, cancellationToken);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.CANCELLED, userId = friendUserId });

            return new FriendRequest("", 0, "", friendUserId, "");
        }

        public void CancelRequestByUserId(string friendUserId) =>
            apiBridge.CancelRequestByUserId(friendUserId);

        public async UniTask<FriendRequest> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CancelFriendshipConfirmationPayload payload = await apiBridge.CancelRequestAsync(friendRequestId, cancellationToken);

            var friendRequest = ToFriendRequest(payload.friendRequest);

            friendRequests.Remove(friendRequest.FriendRequestId);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.CANCELLED, userId = friendRequest.To });

            return friendRequest;
        }

        public void AcceptFriendship(string friendUserId) =>
            apiBridge.AcceptFriendship(friendUserId);

        private void FriendNotFound(string name) =>
            OnFriendNotFound?.Invoke(name);

        private void AddFriendsWithDirectMessages(AddFriendsWithDirectMessagesPayload friendsWithDMs)
        {
            TotalFriendsWithDirectMessagesCount = friendsWithDMs.totalFriendsWithDirectMessages;

            var friendIds = friendsWithDMs.currentFriendsWithDirectMessages.Select(messages => messages.userId);

            foreach (var friendId in friendIds)
            {
                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.APPROVED, userId = friendId });
            }

            OnAddFriendsWithDirectMessages?.Invoke(friendsWithDMs.currentFriendsWithDirectMessages.ToList());
        }

        private void UpdateUserPresence(UserStatus newUserStatus)
        {
            if (!friends.ContainsKey(newUserStatus.userId)) return;

            // Kernel doesn't send the friendship status on this call, we have to keep it or it gets defaulted
            newUserStatus.friendshipStatus = friends[newUserStatus.userId].friendshipStatus;
            UpdateUserStatus(newUserStatus);
        }

        // TODO (Joni): this should be called after AddFriendRequests / RemoveFriendRequest
        private void UpdateTotalFriendRequests(UpdateTotalFriendRequestsPayload msg)
        {
            TotalReceivedFriendRequestCount = msg.totalReceivedRequests;
            TotalSentFriendRequestCount = msg.totalSentRequests;
            OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);
        }

        private void UpdateTotalFriends(UpdateTotalFriendsPayload msg)
        {
            TotalFriendCount = msg.totalFriends;
        }

        private void UpdateUserStatus(UserStatus newUserStatus)
        {
            if (!friends.ContainsKey(newUserStatus.userId))
            {
                friends.Add(newUserStatus.userId, newUserStatus);
                OnUpdateUserStatus?.Invoke(newUserStatus.userId, newUserStatus);
            }
            else
            {
                if (!friends[newUserStatus.userId].Equals(newUserStatus))
                {
                    friends[newUserStatus.userId] = newUserStatus;
                    OnUpdateUserStatus?.Invoke(newUserStatus.userId, newUserStatus);
                }
            }
        }

        private void ReceiveFriendRequest(FriendRequestPayload msg)
        {
            FriendRequest request = ToFriendRequest(msg);

            friendRequests[msg.friendRequestId] = request;
            OnFriendRequestReceived?.Invoke(request);
        }

        // TODO (Joni): Replace by according friend event from RPC Bridge
        private void HandleUpdateFriendshipStatus(FriendshipUpdateStatusMessage msg)
        {
            UpdateFriendshipStatus(msg);

            var friendRequest = GetAllocatedFriendRequestByUser(msg.userId);

            if (msg.action == FriendshipAction.APPROVED)
                OnSentFriendRequestApproved?.Invoke(friendRequest);
        }

        private void UpdateFriendshipStatus(FriendshipUpdateStatusMessage msg)
        {
            var friendshipStatus = ToFriendshipStatus(msg.action);
            var userId = msg.userId;

            if (friends.ContainsKey(userId) && friends[userId].friendshipStatus == friendshipStatus)
                return;

            if (!friends.ContainsKey(userId))
            {
                friends.Add(userId, new UserStatus { userId = userId });

                // Not adding the user to friendsSortedByName because this is only called in the legacy flow of a friendship
            }

            if (ItsAnOutdatedUpdate(userId, friendshipStatus))
                return;

            friends[userId].friendshipStatus = friendshipStatus;

            if (friendshipStatus == FriendshipStatus.NOT_FRIEND)
            {
                var friend = this.friends[userId];
                friends.Remove(userId);

                if (!string.IsNullOrEmpty(friend.userName)) { friendsSortedByName.Remove(friend.userName); }
            }

            OnUpdateFriendship?.Invoke(userId, msg.action);
        }

        private bool ItsAnOutdatedUpdate(string userId, FriendshipStatus friendshipStatus)
        {
            return friendshipStatus == FriendshipStatus.REQUESTED_FROM
                   && friends[userId].friendshipStatus == FriendshipStatus.FRIEND;
        }

        private static FriendshipStatus ToFriendshipStatus(FriendshipAction action)
        {
            switch (action)
            {
                case FriendshipAction.NONE:
                    break;
                case FriendshipAction.APPROVED:
                    return FriendshipStatus.FRIEND;
                case FriendshipAction.REJECTED:
                    return FriendshipStatus.NOT_FRIEND;
                case FriendshipAction.CANCELLED:
                    return FriendshipStatus.NOT_FRIEND;
                case FriendshipAction.REQUESTED_FROM:
                    return FriendshipStatus.REQUESTED_FROM;
                case FriendshipAction.REQUESTED_TO:
                    return FriendshipStatus.REQUESTED_TO;
                case FriendshipAction.DELETED:
                    return FriendshipStatus.NOT_FRIEND;
            }

            return FriendshipStatus.NOT_FRIEND;
        }

        private void AddFriends(IEnumerable<string> friendIds)
        {
            foreach (var friendId in friendIds)
            {
                if (!friends.ContainsKey(friendId))
                    friends.Add(friendId, new UserStatus { userId = friendId });

                friends[friendId].friendshipStatus = ToFriendshipStatus(FriendshipAction.APPROVED);
            }
        }

        private FriendRequest ToFriendRequest(FriendRequestPayload friendRequest) =>
            new FriendRequest(
                friendRequest.friendRequestId,
                friendRequest.timestamp,
                friendRequest.from,
                friendRequest.to,
                friendRequest.messageBody);

        // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        private void AddFriendRequests(AddFriendRequestsPayload msg)
        {
            TotalReceivedFriendRequestCount = msg.totalReceivedFriendRequests;
            TotalSentFriendRequestCount = msg.totalSentFriendRequests;
            OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);

            foreach (var userId in msg.requestedFrom)
            {
                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.REQUESTED_FROM, userId = userId });
            }

            foreach (var userId in msg.requestedTo)
            {
                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.REQUESTED_TO, userId = userId });
            }
        }

        private void AddIncomingFriendRequest(FriendRequest friendRequest)
        {
            this.incomingFriendRequestsById[friendRequest.FriendRequestId] = friendRequest;
            this.incomingFriendRequestsByTimestamp[friendRequest.Timestamp] = friendRequest;

            OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);
        }

        private void AddOutgoingFriendRequest(FriendRequest friendRequest)
        {
            this.outgoingFriendRequestsById[friendRequest.FriendRequestId] = friendRequest;
            this.outgoingFriendRequestsByTimestamp[friendRequest.Timestamp] = friendRequest;

            OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);
        }

        private FriendshipAction ToFriendshipAction(FriendshipStatus status)
        {
            switch (status)
            {
                case FriendshipStatus.FRIEND:
                    return FriendshipAction.APPROVED;
                case FriendshipStatus.NOT_FRIEND:
                default:
                    return FriendshipAction.NONE;
                case FriendshipStatus.REQUESTED_TO:
                    return FriendshipAction.REQUESTED_TO;
                case FriendshipStatus.REQUESTED_FROM:
                    return FriendshipAction.REQUESTED_FROM;
            }
        }

        private async UniTask WaitForFeatureFlagsToBeInitialized(CancellationToken cancellationToken)
        {
            if (featureFlagsInitializedTask == null)
            {
                featureFlagsInitializedTask = new UniTaskCompletionSource();

                void CompleteTaskAndUnsubscribe(FeatureFlag current, FeatureFlag previous)
                {
                    dataStore.featureFlags.flags.OnChange -= CompleteTaskAndUnsubscribe;
                    featureFlagsInitializedTask.TrySetResult();
                }

                dataStore.featureFlags.flags.OnChange += CompleteTaskAndUnsubscribe;
            }

            await featureFlagsInitializedTask.Task.AttachExternalCancellation(cancellationToken);
        }
    }
}
