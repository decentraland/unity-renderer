using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FriendsController : MonoBehaviour, IFriendsController
{
    public static bool VERBOSE = false;
    public static FriendsController i { get; private set; }

    public int FriendCount => friends.Count(f => f.Value.friendshipStatus == FriendshipStatus.FRIEND);

    void Awake() { i = this; }

    private const bool KERNEL_CAN_REMOVE_ENTRIES = false;
    public bool IsInitialized { get; private set; } = false;

    public int ReceivedRequestCount =>
        friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);
    
    public Dictionary<string, UserStatus> friends = new Dictionary<string, UserStatus>();

    [System.Serializable]
    public class UserStatus
    {
        [System.Serializable]
        public class Realm
        {
            public string serverName;
            public string layer;
        }

        public Realm realm;
        public Vector2 position;
        public string userId;
        public FriendshipStatus friendshipStatus;
        public PresenceStatus presence;
        [NonSerialized] public DateTime friendshipStartedTime;
    }

    [System.Serializable]
    public class FriendshipInitializationMessage
    {
        [Serializable]
        public class UnseenRequests
        {
            public int count;
            public long lastSeenTimestamp;
        }

        [Serializable]
        public class UnseenPrivateMessageList
        {
        }

        [Serializable]
        public class UnseenPrivateMessage
        {
            public string userId;
            public int count;
            public long lastSeenTimestamp;
        }
        
        public string[] currentFriends;
        public string[] requestedTo;
        public string[] requestedFrom;
        public UnseenRequests unseenRequests;
        public UnseenPrivateMessage[] unseenPrivateMessages;
    }

    [System.Serializable]
    public class FriendshipUpdateStatusMessage
    {
        public string userId;
        public FriendshipAction action;
    }

    public UserStatus GetUserStatus(string userId)
    {
        if (!friends.ContainsKey(userId))
            return new UserStatus() { userId = userId, friendshipStatus = FriendshipStatus.NOT_FRIEND };

        return friends[userId];
    }

    public bool ContainsStatus(string friendId, FriendshipStatus status)
    {
        if (!friends.ContainsKey(friendId)) return false;
        return friends[friendId].friendshipStatus == status;
    }

    public event Action<string, UserStatus> OnUpdateUserStatus;
    public event Action<string, FriendshipAction> OnUpdateFriendship;
    public event Action<string> OnFriendNotFound;
    public event Action OnInitialized;
    public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;

    public Dictionary<string, UserStatus> GetAllocatedFriends() { return new Dictionary<string, UserStatus>(friends); }

    public void RejectFriendship(string friendUserId)
    {
        UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
        {
            userId = friendUserId,
            action = FriendshipAction.REJECTED
        });
    }

    public bool IsFriend(string userId) => friends.ContainsKey(userId);
    
    public void RemoveFriend(string friendId)
    {
        UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
        {
            action = FriendshipAction.DELETED,
            userId = friendId
        });
    }

    public void GetFriendsAsync(int limit, int skip)
    {
        throw new NotImplementedException();
    }

    public void GetFriendsAsync(string usernameOrId)
    {
        throw new NotImplementedException();
    }

    public void GetFriendRequestsAsync(int sentLimit, long sentFromTimestamp, int receivedLimit, long receivedFromTimestamp)
    {
        throw new NotImplementedException();
    }

    public void GetFriendsWithDirectMessages(int limit, long fromTimestamp) { }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit) { }

    public void RequestFriendship(string friendUserId)
    {
        UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
        {
            userId = friendUserId,
            action = FriendshipAction.REQUESTED_TO
        });
    }

    public void CancelRequest(string friendUserId)
    {
        UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
        {
            userId = friendUserId,
            action = FriendshipAction.CANCELLED
        });
    }

    public void AcceptFriendship(string friendUserId)
    {
        UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
        {
            userId = friendUserId,
            action = FriendshipAction.APPROVED
        });
    }

    public void FriendNotFound(string name) { OnFriendNotFound?.Invoke(name); }

    public void InitializeFriends(string json)
    {
        if (IsInitialized)
            return;

        IsInitialized = true;

        FriendshipInitializationMessage msg = JsonUtility.FromJson<FriendshipInitializationMessage>(json);
        HashSet<string> processedIds = new HashSet<string>();

        foreach (var userId in msg.currentFriends)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage()
                { action = FriendshipAction.APPROVED, userId = userId });
            if (!processedIds.Contains(userId))
                processedIds.Add(userId);
        }

        foreach (var userId in msg.requestedFrom)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage()
                { action = FriendshipAction.REQUESTED_FROM, userId = userId });
            if (!processedIds.Contains(userId))
                processedIds.Add(userId);
        }

        foreach (var userId in msg.requestedTo)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage()
                { action = FriendshipAction.REQUESTED_TO, userId = userId });
            if (!processedIds.Contains(userId))
                processedIds.Add(userId);
        }

        Queue<string> newFriends = new Queue<string>();

        foreach (var kvp in friends)
        {
            if (!processedIds.Contains(kvp.Key))
            {
                newFriends.Enqueue(kvp.Key);
            }
        }

        while (newFriends.Count > 0)
        {
            var userId = newFriends.Dequeue();

            if (KERNEL_CAN_REMOVE_ENTRIES)
            {
                UpdateFriendshipStatus(new FriendshipUpdateStatusMessage()
                    { action = FriendshipAction.NONE, userId = userId });
            }

            if (friends.ContainsKey(userId))
                friends.Remove(userId);
        }

        OnInitialized?.Invoke();
    }

    // called by kernel
    [UsedImplicitly]
    public void AddFriends(string json)
    {
        var msg = JsonUtility.FromJson<AddFriendsPayload>(json);
    }

    // called by kernel
    [UsedImplicitly]
    public void AddFriendRequests(string json)
    {
        var msg = JsonUtility.FromJson<AddFriendRequestsPayload>(json);
    }

    // called by kernel
    [UsedImplicitly]
    public void AddFriendsWithDirectMessages(string json)
    {
        var friendsWithDMs = JsonUtility.FromJson<AddFriendsWithDirectMessagesPayload>(json);
        OnAddFriendsWithDirectMessages?.Invoke(friendsWithDMs.currentFriendsWithDirectMessages.ToList());
    }

    public void UpdateUserStatus(UserStatus newUserStatus)
    {
        if (!friends.ContainsKey(newUserStatus.userId))
        {
            friends.Add(newUserStatus.userId, newUserStatus);
        }
        else
        {
            friends[newUserStatus.userId] = newUserStatus;
        }

        OnUpdateUserStatus?.Invoke(newUserStatus.userId, newUserStatus);
    }

    public void UpdateUserPresence(string json)
    {
        UserStatus newUserStatus = JsonUtility.FromJson<UserStatus>(json);

        if (!friends.ContainsKey(newUserStatus.userId))
            return;

        // Kernel doesn't send the friendship status on this call, we have to keep it or it gets defaulted
        newUserStatus.friendshipStatus = friends[newUserStatus.userId].friendshipStatus;

        UpdateUserStatus(newUserStatus);
    }

    public void UpdateFriendshipStatus(FriendshipUpdateStatusMessage msg)
    {
        var friendshipStatus = ToFriendshipStatus(msg.action);
        var userId = msg.userId;

        if (friends.ContainsKey(userId) && friends[userId].friendshipStatus == friendshipStatus)
            return;

        if (!friends.ContainsKey(userId))
            friends.Add(userId, new UserStatus());

        if (ItsAnOutdatedUpdate(userId, friendshipStatus))
            return;

        friends[userId].friendshipStatus = friendshipStatus;

        if (friendshipStatus == FriendshipStatus.FRIEND)
            friends[userId].friendshipStartedTime = DateTime.UtcNow;

        if (VERBOSE)
            Debug.Log($"Change friend status of {userId} to {friends[userId].friendshipStatus}");

        if (friendshipStatus == FriendshipStatus.NOT_FRIEND)
            friends.Remove(userId);

        OnUpdateFriendship?.Invoke(userId, msg.action);
    }

    public void UpdateFriendshipStatus(string json)
    {
        FriendshipUpdateStatusMessage msg = JsonUtility.FromJson<FriendshipUpdateStatusMessage>(json);
        UpdateFriendshipStatus(msg);
    }

    private bool ItsAnOutdatedUpdate(string userId, FriendshipStatus friendshipStatus)
    {
        return friendshipStatus == FriendshipStatus.REQUESTED_FROM
               && friends[userId].friendshipStatus == FriendshipStatus.FRIEND
               && (DateTime.UtcNow - friends[userId].friendshipStartedTime).TotalSeconds < 5;
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
    
    [ContextMenu("Change user stats to online")]
    public void FakeOnlineFriend()
    {
        var friend = friends.Values.First();
        UpdateUserStatus(new UserStatus
        {
            userId = friend.userId,
            position = friend.position,
            presence = PresenceStatus.ONLINE,
            friendshipStatus = friend.friendshipStatus,
            friendshipStartedTime = friend.friendshipStartedTime
        });
    }
}