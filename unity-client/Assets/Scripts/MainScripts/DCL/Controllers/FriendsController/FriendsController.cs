using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IFriendsController
{
    int friendCount { get; }
    bool isInitialized { get; }
    Dictionary<string, FriendsController.UserStatus> GetFriends();

    event Action OnInitialized;
    event Action<string, FriendshipAction> OnUpdateFriendship;
    event Action<string, FriendsController.UserStatus> OnUpdateUserStatus;
    event Action<string> OnFriendNotFound;
}

public enum PresenceStatus
{
    NONE,
    OFFLINE,
    ONLINE,
    UNAVAILABLE,
}

public enum FriendshipStatus
{
    NONE,
    FRIEND,
    REQUESTED_FROM,
    REQUESTED_TO
}

public enum FriendshipAction
{
    NONE,
    APPROVED,
    REJECTED,
    CANCELLED,
    REQUESTED_FROM,
    REQUESTED_TO,
    DELETED
}

public class FriendsController : MonoBehaviour, IFriendsController
{
    public static bool VERBOSE = false;
    public static FriendsController i { get; private set; }

    public int friendCount => friends.Count(f => f.Value.friendshipStatus == FriendshipStatus.FRIEND);

    void Awake()
    {
        i = this;
    }

    private const bool KERNEL_CAN_REMOVE_ENTRIES = false;
    public bool isInitialized { get; private set; } = false;
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
    }

    [System.Serializable]
    public class FriendshipInitializationMessage
    {
        public string[] currentFriends;
        public string[] requestedTo;
        public string[] requestedFrom;
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
            return new UserStatus() {userId = userId, friendshipStatus = FriendshipStatus.NONE};

        return friends[userId];
    }

    public event System.Action<string, UserStatus> OnUpdateUserStatus;
    public event System.Action<string, FriendshipAction> OnUpdateFriendship;
    public event Action<string> OnFriendNotFound;
    public event Action OnInitialized;

    public Dictionary<string, UserStatus> GetFriends()
    {
        return new Dictionary<string, UserStatus>(friends);
    }

    public void FriendNotFound(string name)
    {
        OnFriendNotFound?.Invoke(name);
    }

    public void InitializeFriends(string json)
    {
        isInitialized = true;
        OnInitialized?.Invoke();

        FriendshipInitializationMessage msg = JsonUtility.FromJson<FriendshipInitializationMessage>(json);
        HashSet<string> processedIds = new HashSet<string>();

        foreach (var userId in msg.currentFriends)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage()
                {action = FriendshipAction.APPROVED, userId = userId});
            if (!processedIds.Contains(userId))
                processedIds.Add(userId);
        }

        foreach (var userId in msg.requestedFrom)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage()
                {action = FriendshipAction.REQUESTED_FROM, userId = userId});
            if (!processedIds.Contains(userId))
                processedIds.Add(userId);
        }

        foreach (var userId in msg.requestedTo)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage()
                {action = FriendshipAction.REQUESTED_TO, userId = userId});
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
                    {action = FriendshipAction.NONE, userId = userId});
            }

            if (friends.ContainsKey(userId))
                friends.Remove(userId);
        }
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

        if (!friends.ContainsKey(newUserStatus.userId)) return;

        // Kernel doesn't send the friendship status on this call, we have to keep it or it gets defaulted
        newUserStatus.friendshipStatus = friends[newUserStatus.userId].friendshipStatus;

        UpdateUserStatus(newUserStatus);
    }

    public void UpdateFriendshipStatus(FriendshipUpdateStatusMessage msg)
    {
        if (!friends.ContainsKey(msg.userId))
        {
            friends.Add(msg.userId, new UserStatus() { });
        }

        switch (msg.action)
        {
            case FriendshipAction.NONE:
                break;
            case FriendshipAction.APPROVED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.FRIEND;
                break;
            case FriendshipAction.REJECTED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.NONE;
                break;
            case FriendshipAction.CANCELLED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.NONE;
                break;
            case FriendshipAction.REQUESTED_FROM:
                friends[msg.userId].friendshipStatus = FriendshipStatus.REQUESTED_FROM;
                break;
            case FriendshipAction.REQUESTED_TO:
                friends[msg.userId].friendshipStatus = FriendshipStatus.REQUESTED_TO;
                break;
            case FriendshipAction.DELETED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.NONE;
                break;
        }

        if (VERBOSE)
            Debug.Log($"Change friend status of {msg.userId} to {friends[msg.userId].friendshipStatus}");

        if (friends[msg.userId].friendshipStatus == FriendshipStatus.NONE)
        {
            friends.Remove(msg.userId);
        }

        OnUpdateFriendship?.Invoke(msg.userId, msg.action);
    }

    public void UpdateFriendshipStatus(string json)
    {
        FriendshipUpdateStatusMessage msg = JsonUtility.FromJson<FriendshipUpdateStatusMessage>(json);
        UpdateFriendshipStatus(msg);
    }
}