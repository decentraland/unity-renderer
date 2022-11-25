using System;
using DCL.Interface;
using DCl.Social.Friends;
using JetBrains.Annotations;
using UnityEngine;

namespace DCL.Social.Friends
{
    public partial class WebInterfaceFriendsApiBridge : MonoBehaviour, IFriendsApiBridge
    {
        public event Action<FriendshipInitializationMessage> OnInitialized;
        public event Action<string> OnFriendNotFound;
        public event Action<AddFriendsPayload> OnFriendsAdded;
        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded;
        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded;
        public event Action<UserStatus> OnUserPresenceUpdated;
        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;
        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated;
        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated;
        
        [PublicAPI]
        public void InitializeFriends(string json) =>
            OnInitialized?.Invoke(JsonUtility.FromJson<FriendshipInitializationMessage>(json));

        [PublicAPI]
        public void FriendNotFound(string name) => OnFriendNotFound?.Invoke(name);

        [PublicAPI]
        public void AddFriends(string json) =>
            OnFriendsAdded?.Invoke(JsonUtility.FromJson<AddFriendsPayload>(json));

        [PublicAPI]
        public void AddFriendRequests(string json) =>
            OnFriendRequestsAdded?.Invoke(JsonUtility.FromJson<AddFriendRequestsPayload>(json));

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

        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            WebInterface.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip) =>
            WebInterface.GetFriendsWithDirectMessages(usernameOrId, limit, skip);

        public void RequestFriendship(string userId)
        {
            WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
            {
                userId = userId,
                action = WebInterface.FriendshipAction.REQUESTED_TO
            });
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