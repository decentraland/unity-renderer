using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCL.Social.Friends
{
    public class FriendsController : IFriendsController
    {
        public static FriendsController i { get; private set; }

        private readonly IFriendsApiBridge apiBridge;
        private readonly Dictionary<string, FriendRequest> friendRequests = new ();

        public event Action<int> OnTotalFriendsUpdated;
        public int AllocatedFriendCount => friends.Count(f => f.Value.friendshipStatus == FriendshipStatus.FRIEND);
        public bool IsInitialized { get; private set; }

        public int ReceivedRequestCount => friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);

        public int TotalFriendCount { get; private set; }
        public int TotalFriendRequestCount => TotalReceivedFriendRequestCount + TotalSentFriendRequestCount;
        public int TotalReceivedFriendRequestCount { get; private set; }
        public int TotalSentFriendRequestCount { get; private set; }
        public int TotalFriendsWithDirectMessagesCount { get; private set; }

        public readonly Dictionary<string, UserStatus> friends = new Dictionary<string, UserStatus>();

        public event Action<string, UserStatus> OnUpdateUserStatus;
        public event Action<string, FriendshipAction> OnUpdateFriendship;
        public event Action<string> OnFriendNotFound;
        public event Action OnInitialized;
        public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
        public event Action<int, int> OnTotalFriendRequestUpdated;
        public event Action<FriendRequest> OnAddFriendRequest;

        public static void CreateSharedInstance(IFriendsApiBridge apiBridge)
        {
            i = new FriendsController(apiBridge);
        }

        public FriendsController(IFriendsApiBridge apiBridge)
        {
            this.apiBridge = apiBridge;
            apiBridge.OnInitialized += Initialize;
            apiBridge.OnFriendNotFound += FriendNotFound;
            apiBridge.OnFriendsAdded += AddFriends;
            apiBridge.OnFriendWithDirectMessagesAdded += AddFriendsWithDirectMessages;
            apiBridge.OnUserPresenceUpdated += UpdateUserPresence;
            apiBridge.OnFriendshipStatusUpdated += UpdateFriendshipStatus;
            apiBridge.OnTotalFriendRequestCountUpdated += UpdateTotalFriendRequests;
            apiBridge.OnTotalFriendCountUpdated += UpdateTotalFriends;
            apiBridge.OnFriendRequestsAdded += AddFriendRequests; // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
            apiBridge.OnFriendRequestAdded += AddFriendRequest;
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

        public async UniTask<FriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody)
        {
            var payload = await apiBridge.RequestFriendshipAsync(friendUserId, messageBody);
            var friendRequestPayload = payload.friendRequest;

            var friendRequest = new FriendRequest(friendRequestPayload.friendRequestId,
                friendRequestPayload.timestamp,
                friendRequestPayload.from,
                friendRequestPayload.to,
                friendRequestPayload.messageBody);

            friendRequests[friendRequest.FriendRequestId] = friendRequest;

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.REQUESTED_TO, userId = friendUserId });

            return friendRequest;
        }

        public void RequestFriendship(string friendUserId) =>
            apiBridge.RequestFriendship(friendUserId);

        public Dictionary<string, UserStatus> GetAllocatedFriends() =>
            new Dictionary<string, UserStatus>(friends);

        public async UniTask<FriendRequest> AcceptFriendshipAsync(string friendRequestId)
        {
            AcceptFriendshipPayload payload = await apiBridge.AcceptFriendshipAsync(friendRequestId);
            FriendRequestPayload requestPayload = payload.FriendRequest;
            var request = ToFriendRequest(requestPayload);
            // NOTE: would it be better to register the new state instead of removing it?
            friendRequests.Remove(friendRequestId);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.APPROVED, userId = request.From });

            return request;
        }

        public void RejectFriendship(string friendUserId) =>
            apiBridge.RejectFriendship(friendUserId);

        public async UniTask<FriendRequest> RejectFriendshipAsync(string friendRequestId)
        {
            RejectFriendshipPayload payload = await apiBridge.RejectFriendshipAsync(friendRequestId);
            FriendRequestPayload requestPayload = payload.FriendRequestPayload;
            var request = ToFriendRequest(requestPayload);
            // NOTE: would it be better to register the new state instead of removing it?
            friendRequests.Remove(friendRequestId);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.REJECTED, userId = request.From });

            return request;
        }

        public bool IsFriend(string userId) =>
            friends.ContainsKey(userId) && friends[userId].friendshipStatus == FriendshipStatus.FRIEND;

        public void RemoveFriend(string friendId) =>
            apiBridge.RemoveFriend(friendId);

        public void GetFriends(int limit, int skip) =>
            apiBridge.GetFriends(limit, skip);

        public void GetFriends(string usernameOrId, int limit) =>
            apiBridge.GetFriends(usernameOrId, limit);

        // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            apiBridge.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public async UniTask<List<FriendRequest>> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
        {
            var payload = await apiBridge.GetFriendRequestsAsync(sentLimit, sentSkip, receivedLimit, receivedSkip);

            TotalReceivedFriendRequestCount = payload.totalReceivedFriendRequests;
            TotalSentFriendRequestCount = payload.totalSentFriendRequests;
            OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);

            List<FriendRequest> receivedFriendRequestsToAdd = new List<FriendRequest>();

            foreach (var friendRequest in payload.requestedFrom)
            {
                var request = ToFriendRequest(friendRequest);
                friendRequests[request.FriendRequestId] = request;
                receivedFriendRequestsToAdd.Add(request);

                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.REQUESTED_FROM, userId = friendRequest.from });
            }

            foreach (var friendRequest in payload.requestedTo)
            {
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

        public FriendRequest GetAllocatedFriendRequestByUser(string userId) =>
            friendRequests.Values.FirstOrDefault(request => request.From == userId || request.To == userId);

        public async UniTask<FriendRequest> CancelRequestByUserIdAsync(string friendUserId)
        {
            FriendRequest request = GetAllocatedFriendRequestByUser(friendUserId);

            if (request != null)
            {
                await apiBridge.CancelRequestAsync(request.FriendRequestId);
                friendRequests.Remove(request.FriendRequestId);
                return request;
            }

            await apiBridge.CancelRequestByUserIdAsync(friendUserId);

            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                { action = FriendshipAction.CANCELLED, userId = friendUserId });

            return new FriendRequest("", 0, "", friendUserId, "");
        }

        public void CancelRequestByUserId(string friendUserId) =>
            apiBridge.CancelRequestByUserId(friendUserId);

        public async UniTask<FriendRequest> CancelRequestAsync(string friendRequestId)
        {
            CancelFriendshipConfirmationPayload payload = await apiBridge.CancelRequestAsync(friendRequestId);
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

        private void AddFriends(AddFriendsPayload msg)
        {
            TotalFriendCount = msg.totalFriends;
            OnTotalFriendsUpdated?.Invoke(TotalFriendCount);
            AddFriends(msg.friends);
        }

        private void AddFriendsWithDirectMessages(AddFriendsWithDirectMessagesPayload friendsWithDMs)
        {
            TotalFriendsWithDirectMessagesCount = friendsWithDMs.totalFriendsWithDirectMessages;
            AddFriends(friendsWithDMs.currentFriendsWithDirectMessages.Select(messages => messages.userId));
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
            OnTotalFriendsUpdated?.Invoke(TotalFriendCount);
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
                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                    { action = FriendshipAction.APPROVED, userId = friendId });
            }
        }

        private FriendRequest ToFriendRequest(FriendRequestPayload friendRequest) =>
            new FriendRequest(
                friendRequest.friendRequestId,
                friendRequest.timestamp,
                friendRequest.@from,
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

        private void AddFriendRequest(FriendRequestPayload friendRequest)
        {
            OnAddFriendRequest?.Invoke(new FriendRequest(
                friendRequest.friendRequestId,
                friendRequest.timestamp,
                friendRequest.from,
                friendRequest.to,
                friendRequest.messageBody));
        }
    }
}
