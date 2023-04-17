using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Interface;
using DCl.Social.Friends;
using JetBrains.Annotations;
using System.Threading;
using UnityEngine;
using System.Threading;

namespace DCL.Social.Friends
{
    public partial class WebInterfaceFriendsApiBridge : MonoBehaviour, IFriendsApiBridge
    {
        private const string GET_FRIENDS_REQUEST_MESSAGE_ID = "GetFriendsRequest";

        public static WebInterfaceFriendsApiBridge GetOrCreate()
        {
            var bridgeObj = GameObject.Find("Main");

            return bridgeObj == null
                ? new GameObject("Main").AddComponent<WebInterfaceFriendsApiBridge>()
                : bridgeObj.GetOrCreateComponent<WebInterfaceFriendsApiBridge>();
        }

        private readonly Dictionary<string, IUniTaskSource> pendingRequests = new ();
        private readonly Dictionary<string, UniTaskCompletionSource<FriendshipUpdateStatusMessage>> updatedFriendshipPendingRequests = new ();

        public event Action<FriendshipInitializationMessage> OnInitialized;
        public event Action<string> OnFriendNotFound;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded; // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded;
        public event Action<UserStatus> OnUserPresenceUpdated;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated;
        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated;
        public event Action<FriendRequestPayload> OnFriendRequestReceived;

        [PublicAPI]
        public void InitializeFriends(string json) =>
            OnInitialized?.Invoke(JsonUtility.FromJson<FriendshipInitializationMessage>(json));

        [PublicAPI]
        public void FriendNotFound(string name) =>
            OnFriendNotFound?.Invoke(name);

        [PublicAPI]
        public void AddFriends(string json)
        {
            var payload = JsonUtility.FromJson<AddFriendsPayload>(json);
            string messageId = GET_FRIENDS_REQUEST_MESSAGE_ID;

            if (!pendingRequests.ContainsKey(messageId))
                return;

            var task = (UniTaskCompletionSource<AddFriendsPayload>)pendingRequests[messageId];

            pendingRequests.Remove(messageId);
            task.TrySetResult(payload);
        }

        // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        [PublicAPI]
        public void AddFriendRequests(string json) =>
            OnFriendRequestsAdded?.Invoke(JsonUtility.FromJson<AddFriendRequestsPayload>(json));

        [PublicAPI]
        public void AddFriendRequest(string json)
        {
            var payload = JsonUtility.FromJson<FriendRequestPayload>(json);
            OnFriendRequestReceived?.Invoke(payload);
        }

        [PublicAPI]
        public void AddFriendsWithDirectMessages(string json) =>
            OnFriendWithDirectMessagesAdded?.Invoke(JsonUtility.FromJson<AddFriendsWithDirectMessagesPayload>(json));

        [PublicAPI]
        public void UpdateUserPresence(string json) =>
            OnUserPresenceUpdated?.Invoke(JsonUtility.FromJson<UserStatus>(json));

        [PublicAPI]
        public void UpdateFriendshipStatus(string json)
        {
            FriendshipUpdateStatusMessage msg = JsonUtility.FromJson<FriendshipUpdateStatusMessage>(json);
            string userId = msg.userId;

            if (updatedFriendshipPendingRequests.ContainsKey(userId))
                updatedFriendshipPendingRequests[userId].TrySetResult(msg);

            updatedFriendshipPendingRequests.Remove(userId);

            OnFriendshipStatusUpdated?.Invoke(msg);
        }

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

            var task = (UniTaskCompletionSource<RequestFriendshipConfirmationPayload>)pendingRequests[messageId];

            pendingRequests.Remove(messageId);
            task.TrySetResult(payload);
        }

        [PublicAPI]
        public void RequestFriendshipError(string json)
        {
            var payload = JsonUtility.FromJson<RequestFriendshipErrorPayload>(json);
            var messageId = payload.messageId;
            if (!pendingRequests.ContainsKey(messageId)) return;

            var task = (UniTaskCompletionSource<RequestFriendshipConfirmationPayload>)pendingRequests[messageId];

            pendingRequests.Remove(messageId);
            task.TrySetException(new FriendshipException((FriendRequestErrorCodes)payload.errorCode));
        }

        [PublicAPI]
        public void CancelFriendshipConfirmation(string json)
        {
            var payload = JsonUtility.FromJson<CancelFriendshipConfirmationPayload>(json);
            string messageId = payload.messageId;
            if (!pendingRequests.ContainsKey(messageId)) return;

            var task = (UniTaskCompletionSource<CancelFriendshipConfirmationPayload>)pendingRequests[messageId];

            pendingRequests.Remove(messageId);
            task.TrySetResult(payload);

            OnFriendshipStatusUpdated?.Invoke(new FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.CANCELLED,
                userId = payload.friendRequest.to
            });
        }

        [PublicAPI]
        public void CancelFriendshipError(string json)
        {
            var payload = JsonUtility.FromJson<CancelFriendshipErrorPayload>(json);
            var messageId = payload.messageId;
            if (!pendingRequests.ContainsKey(messageId)) return;

            var task = (UniTaskCompletionSource<CancelFriendshipConfirmationPayload>)pendingRequests[messageId];

            pendingRequests.Remove(messageId);
            task.TrySetException(new FriendshipException((FriendRequestErrorCodes)payload.errorCode));
        }

        public void RejectFriendship(string userId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                action = WebInterface.FriendshipAction.REJECTED,
                userId = userId
            });
        }

        public UniTask<RejectFriendshipPayload> RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken) =>
            throw new NotImplementedException("Already implemented in RPCFriendsApiBridge");

        public void RemoveFriend(string userId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                action = WebInterface.FriendshipAction.DELETED,
                userId = userId
            });
        }

        public UniTask<AddFriendsPayload> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = new UniTaskCompletionSource<AddFriendsPayload>();

            pendingRequests[GET_FRIENDS_REQUEST_MESSAGE_ID] = task;
            WebInterface.GetFriends(limit, skip);

            return task.Task.AttachExternalCancellation(cancellationToken);
        }

        public UniTask<AddFriendsPayload> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = new UniTaskCompletionSource<AddFriendsPayload>();

            pendingRequests[GET_FRIENDS_REQUEST_MESSAGE_ID] = task;
            WebInterface.GetFriends(usernameOrId, limit);

            return task.Task.AttachExternalCancellation(cancellationToken);
        }

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

        public UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken) =>
            throw new NotImplementedException("Already implemented in RPCFriendsApiBridge");

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip) =>
            WebInterface.GetFriendsWithDirectMessages(usernameOrId, limit, skip);

        public void RequestFriendship(string friendUserId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                action = WebInterface.FriendshipAction.REQUESTED_TO,
                userId = friendUserId,
            });
        }

        public UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            return task.Task.AttachExternalCancellation(cancellationToken);
        }

        public UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = new UniTaskCompletionSource<CancelFriendshipConfirmationPayload>();

            // TODO: optimize unique id length for performance reasons
            var messageId = Guid.NewGuid().ToString("N");
            pendingRequests[messageId] = task;

            WebInterface.SendMessage("CancelFriendship",
                new CancelFriendshipPayload
                {
                    messageId = messageId,
                    friendRequestId = friendRequestId
                });

            return task.Task.AttachExternalCancellation(cancellationToken);
        }

        public UniTask CancelRequestByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = updatedFriendshipPendingRequests.ContainsKey(userId)
                ? updatedFriendshipPendingRequests[userId]
                : new UniTaskCompletionSource<FriendshipUpdateStatusMessage>();

            updatedFriendshipPendingRequests[userId] = task;

            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                userId = userId,
                action = WebInterface.FriendshipAction.CANCELLED
            });

            return task.Task.AttachExternalCancellation(cancellationToken);
        }

        public void CancelRequestByUserId(string userId)
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

        public UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken) =>
            throw new NotImplementedException("Already implemented in RPCFriendsApiBridge");

        public UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken) =>
            throw new NotImplementedException("Already implemented in RPCFriendsApiBridge");
    }
}
