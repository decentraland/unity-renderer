using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DCL.Social.Friends
{
    public class FriendsController : IFriendsController
    {
        // TODO: remove singleton, rename to FriendsService and add to service locator
        public static FriendsController i { get; private set; }

        private readonly IFriendsApiBridge apiBridge;
        private readonly Dictionary<string, FriendRequest> friendRequests = new ();
        private readonly Dictionary<string, UserStatus> friends = new ();

        public int AllocatedFriendCount => friends.Count(f => f.Value.friendshipStatus == FriendshipStatus.FRIEND);
        public bool IsInitialized { get; private set; }

        public int ReceivedRequestCount => friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);

        public int TotalFriendCount { get; private set; }
        public int TotalFriendRequestCount => TotalReceivedFriendRequestCount + TotalSentFriendRequestCount;
        public int TotalReceivedFriendRequestCount { get; private set; }
        public int TotalSentFriendRequestCount { get; private set; }
        public int TotalFriendsWithDirectMessagesCount { get; private set; }

        public event Action<string, UserStatus> OnUpdateUserStatus;
        public event Action<string, FriendshipAction> OnUpdateFriendship;
        public event Action<string> OnFriendNotFound;
        public event Action OnInitialized;
        public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
        public event Action<int, int> OnTotalFriendRequestUpdated;
        public event Action<FriendRequest> OnFriendRequestReceived;
        public event Action<FriendRequest> OnSentFriendRequestApproved;

        public static void CreateSharedInstance(IFriendsApiBridge apiBridge)
        {
            i = new FriendsController(apiBridge);
        }

        public FriendsController(IFriendsApiBridge apiBridge)
        {
            this.apiBridge = apiBridge;
            apiBridge.OnInitialized += Initialize;
            apiBridge.OnFriendNotFound += FriendNotFound;
            apiBridge.OnFriendWithDirectMessagesAdded += AddFriendsWithDirectMessages;
            apiBridge.OnUserPresenceUpdated += UpdateUserPresence;
            apiBridge.OnFriendshipStatusUpdated += HandleUpdateFriendshipStatus;
            apiBridge.OnTotalFriendRequestCountUpdated += UpdateTotalFriendRequests;
            apiBridge.OnTotalFriendCountUpdated += UpdateTotalFriends;
            apiBridge.OnFriendRequestsAdded += AddFriendRequests; // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
            apiBridge.OnFriendRequestReceived += ReceiveFriendRequest;
        }

        private void Initialize(FriendshipInitializationMessage msg)
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

            RequestFriendshipConfirmationPayload payload = await apiBridge.RequestFriendshipAsync(friendUserId, messageBody, cancellationToken);

            FriendRequestPayload friendRequestPayload = payload.friendRequest;

            FriendRequest friendRequest = new (friendRequestPayload.friendRequestId,
                friendRequestPayload.timestamp,
                friendRequestPayload.from,
                friendRequestPayload.to,
                friendRequestPayload.messageBody);

            friendRequests[friendRequest.FriendRequestId] = friendRequest;

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

            RejectFriendshipPayload payload = await apiBridge.RejectFriendshipAsync(friendRequestId, cancellationToken);

            FriendRequestPayload requestPayload = payload.FriendRequestPayload;
            var request = ToFriendRequest(requestPayload);
            friendRequests.Remove(request.FriendRequestId);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.REJECTED, userId = request.From });

            return request;
        }

        public bool IsFriend(string userId) =>
            friends.ContainsKey(userId) && friends[userId].friendshipStatus == FriendshipStatus.FRIEND;

        public void RemoveFriend(string friendId) =>
            apiBridge.RemoveFriend(friendId);

        public async UniTask<string[]> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default)
        {
            var payload = await apiBridge.GetFriendsAsync(limit, skip, cancellationToken);
            await UniTask.SwitchToMainThread();

            TotalFriendCount = payload.totalFriends;
            AddFriends(payload.friends);

            return payload.friends;
        }

        public async UniTask<string[]> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default)
        {
            var payload = await apiBridge.GetFriendsAsync(usernameOrId, limit, cancellationToken);
            await UniTask.SwitchToMainThread();

            TotalFriendCount = payload.totalFriends;
            AddFriends(payload.friends);

            return payload.friends;
        }

        // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            apiBridge.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public async UniTask<List<FriendRequest>> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

        public FriendRequest GetAllocatedFriendRequest(string friendRequestId) =>
            friendRequests.ContainsKey(friendRequestId) ? friendRequests[friendRequestId] : null;

        public FriendRequest GetAllocatedFriendRequestByUser(string userId)
        {
            long max = long.MinValue;
            FriendRequest result = null;

            foreach (var request in friendRequests.Values)
            {
                if (request.From != userId && request.To != userId) continue;
                if (request.Timestamp <= max) continue;
                result = request;
                max = request.Timestamp;
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
            friendRequestId = friendRequest.FriendRequestId;
            friendRequests.Remove(friendRequestId);

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
                friends.Add(userId, new UserStatus { userId = userId });

            if (ItsAnOutdatedUpdate(userId, friendshipStatus))
                return;

            friends[userId].friendshipStatus = friendshipStatus;

            if (friendshipStatus == FriendshipStatus.NOT_FRIEND)
                friends.Remove(userId);

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
    }
}
