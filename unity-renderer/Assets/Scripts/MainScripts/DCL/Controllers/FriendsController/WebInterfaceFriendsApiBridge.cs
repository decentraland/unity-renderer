using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCl.Social.Friends;
using JetBrains.Annotations;
using UnityEngine;

namespace DCL.Social.Friends
{
    public partial class WebInterfaceFriendsApiBridge : MonoBehaviour, IFriendsApiBridge
    {
        private readonly Dictionary<string, IUniTaskSource> pendingRequests =
            new Dictionary<string, IUniTaskSource>();

        public event Action<FriendshipInitializationMessage> OnInitialized;
        public event Action<string> OnFriendNotFound;
        public event Action<AddFriendsPayload> OnFriendsAdded;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded; // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded;
        public event Action<UserStatus> OnUserPresenceUpdated;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated;
        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestAdded;

        [PublicAPI]
        public void InitializeFriends(string json) =>
            OnInitialized?.Invoke(JsonUtility.FromJson<FriendshipInitializationMessage>(json));

        [PublicAPI]
        public void FriendNotFound(string name) => OnFriendNotFound?.Invoke(name);

        [PublicAPI]
        public void AddFriends(string json) =>
            OnFriendsAdded?.Invoke(JsonUtility.FromJson<AddFriendsPayload>(json));

        // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        [PublicAPI]
        public void AddFriendRequests(string json) =>
            OnFriendRequestsAdded?.Invoke(JsonUtility.FromJson<AddFriendRequestsPayload>(json));

        [PublicAPI]
        public void AddFriendRequest(string json)
        {
            var payload = JsonUtility.FromJson<FriendRequestPayload>(json);
            OnFriendRequestAdded?.Invoke(payload);
        }

        [PublicAPI]
        public void AddFriendsWithDirectMessages(string json) =>
            OnFriendWithDirectMessagesAdded?.Invoke(JsonUtility.FromJson<AddFriendsWithDirectMessagesPayload>(json));

        [PublicAPI]
        public void UpdateUserPresence(string json) =>
            OnUserPresenceUpdated?.Invoke(JsonUtility.FromJson<UserStatus>(json));

        [PublicAPI]
        public void UpdateFriendshipStatus(string json) =>
            OnFriendshipStatusUpdated?.Invoke(JsonUtility.FromJson<FriendshipUpdateStatusMessage>(json));

        [PublicAPI]
        public void UpdateTotalFriendRequests(string json) =>
            OnTotalFriendRequestCountUpdated?.Invoke(JsonUtility.FromJson<UpdateTotalFriendRequestsPayload>(json));

        [PublicAPI]
        public void UpdateTotalFriends(string json) =>
            OnTotalFriendCountUpdated?.Invoke(JsonUtility.FromJson<UpdateTotalFriendsPayload>(json));

        [PublicAPI]
        public void RequestFriendshipConfirmation(string json)
        {
            var payload = JsonUtility.FromJson<RequestFriendshipConfirmationPayload>(json);
            var messageId = payload.messageId;
            if (!pendingRequests.ContainsKey(messageId)) return;
            var task =
                (UniTaskCompletionSource<RequestFriendshipConfirmationPayload>) pendingRequests[messageId];
            pendingRequests.Remove(messageId);
            task.TrySetResult(payload);
        }

        [PublicAPI]
        public void RequestFriendshipError(string json)
        {
            var payload = JsonUtility.FromJson<RequestFriendshipErrorPayload>(json);
            var messageId = payload.messageId;
            if (!pendingRequests.ContainsKey(messageId)) return;
            var task =
                (UniTaskCompletionSource<RequestFriendshipConfirmationPayload>) pendingRequests[messageId];
            pendingRequests.Remove(messageId);
            task.TrySetException(new FriendshipException((FriendRequestErrorCodes) payload.errorCode));
        }
        
        public void RejectFriendship(string userId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                action = WebInterface.FriendshipAction.REJECTED,
                userId = userId
            });
        }

        public void RemoveFriend(string userId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                action = WebInterface.FriendshipAction.DELETED,
                userId = userId
            });
        }

        public void GetFriends(int limit, int skip) =>
            WebInterface.GetFriends(limit, skip);

        public void GetFriends(string usernameOrId, int limit) =>
            WebInterface.GetFriends(usernameOrId, limit);

        // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
        {
            WebInterface.SendMessage("GetFriendRequests", new GetFriendRequestsPayload
            {
                receivedSkip = receivedSkip,
                receivedLimit = receivedLimit,
                sentSkip = sentSkip,
                sentLimit = sentLimit
            });
        }

        public UniTask<AddFriendRequestsV2Payload> GetFriendRequestsV2(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
        {
            // Already implemented in RPCFriendsApiBridge...
            return UniTask.FromResult(new AddFriendRequestsV2Payload());
        }

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip) =>
            WebInterface.GetFriendsWithDirectMessages(usernameOrId, limit, skip);

        public UniTask<RequestFriendshipConfirmationPayload> RequestFriendship(string userId, string messageBody)
        {
            var task = new UniTaskCompletionSource<RequestFriendshipConfirmationPayload>();
            // TODO: optimize unique id length for performance reasons
            var messageId = Guid.NewGuid().ToString("N");
            pendingRequests[messageId] = task;
            
            WebInterface.SendMessage("RequestFriendship", new RequestFriendshipPayload
            {
                messageId = messageId,
                messageBody = messageBody,
                userId = userId
            });
            return task.Task;
        }

        public void CancelRequest(string userId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                userId = userId,
                action = WebInterface.FriendshipAction.CANCELLED
            });
        }

        public void AcceptFriendship(string userId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                userId = userId,
                action = WebInterface.FriendshipAction.APPROVED
            });
        }
    }
}
