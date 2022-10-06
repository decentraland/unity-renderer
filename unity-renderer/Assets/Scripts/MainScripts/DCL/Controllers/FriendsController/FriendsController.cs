using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using DCL.Friends.WebApi;
using DCL.Interface;
using UnityEngine;

public class FriendsController : MonoBehaviour, IFriendsController
{
    private const bool VERBOSE = false;
    public static FriendsController i { get; private set; }

    public event Action<int> OnTotalFriendsUpdated;
    public int AllocatedFriendCount => friends.Count(f => f.Value.friendshipStatus == FriendshipStatus.FRIEND);

    private void Awake()
    {
        i = this;
    }

    public bool IsInitialized { get; private set; }

    public int ReceivedRequestCount =>
        friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);

    public int TotalFriendCount { get; private set; }
    public int TotalFriendRequestCount => TotalReceivedFriendRequestCount + TotalSentFriendRequestCount;
    public int TotalReceivedFriendRequestCount { get; private set; }
    public int TotalSentFriendRequestCount { get; private set; }
    public int TotalFriendsWithDirectMessagesCount { get; private set; }

    public readonly Dictionary<string, UserStatus> friends = new Dictionary<string, UserStatus>();

    public UserStatus GetUserStatus(string userId)
    {
        if (!friends.ContainsKey(userId))
            return new UserStatus {userId = userId, friendshipStatus = FriendshipStatus.NOT_FRIEND};

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
    public event Action<int, int> OnTotalFriendRequestUpdated;

    public Dictionary<string, UserStatus> GetAllocatedFriends()
    {
        return new Dictionary<string, UserStatus>(friends);
    }

    public void RejectFriendship(string friendUserId)
    {
        WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
        {
            action = WebInterface.FriendshipAction.REJECTED,
            userId = friendUserId
        });
    }

    public bool IsFriend(string userId) => friends.ContainsKey(userId);

    public void RemoveFriend(string friendId)
    {
        WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
        {
            action = WebInterface.FriendshipAction.DELETED,
            userId = friendId
        });
    }

    public void GetFriends(int limit, int skip) =>
        WebInterface.GetFriends(limit, skip);

    public void GetFriends(string usernameOrId, int limit)
    {
        WebInterface.GetFriends(usernameOrId, limit);
    }

    public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit,
        int receivedSkip)
    {
        WebInterface.GetFriendRequests(sentLimit, sentSkip, receivedLimit,
            receivedSkip);
    }

    public void GetFriendsWithDirectMessages(int limit, int skip)
    {
        WebInterface.GetFriendsWithDirectMessages("", limit, skip);
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
        WebInterface.GetFriendsWithDirectMessages(userNameOrId, limit, 0);
    }

    public void RequestFriendship(string friendUserId)
    {
        WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
        {
            userId = friendUserId,
            action = WebInterface.FriendshipAction.REQUESTED_TO
        });
    }

    public void CancelRequest(string friendUserId)
    {
        WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
        {
            userId = friendUserId,
            action = WebInterface.FriendshipAction.CANCELLED
        });
    }

    public void AcceptFriendship(string friendUserId)
    {
        WebInterface.UpdateFriendshipStatus(new WebInterface.FriendshipUpdateStatusMessage
        {
            userId = friendUserId,
            action = WebInterface.FriendshipAction.APPROVED
        });
    }

    // called by kernel
    [UsedImplicitly]
    public void FriendNotFound(string name)
    {
        OnFriendNotFound?.Invoke(name);
    }

    // called by kernel
    [UsedImplicitly]
    public void InitializeFriends(string json)
    {
        if (IsInitialized)
            return;

        IsInitialized = true;

        var msg = JsonUtility.FromJson<FriendshipInitializationMessage>(json);

        TotalReceivedFriendRequestCount = msg.totalReceivedRequests;
        OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);
        OnInitialized?.Invoke();
    }

    // called by kernel
    [UsedImplicitly]
    public void AddFriends(string json)
    {
        var msg = JsonUtility.FromJson<AddFriendsPayload>(json);

        TotalFriendCount = msg.totalFriends;
        OnTotalFriendsUpdated?.Invoke(TotalFriendCount);
        
        AddFriends(msg.friends);
    }

    // called by kernel
    [UsedImplicitly]
    public void AddFriendRequests(string json)
    {
        var msg = JsonUtility.FromJson<AddFriendRequestsPayload>(json);

        TotalReceivedFriendRequestCount = msg.totalReceivedFriendRequests;
        TotalSentFriendRequestCount = msg.totalSentFriendRequests;
        OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);

        foreach (var userId in msg.requestedFrom)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                {action = FriendshipAction.REQUESTED_FROM, userId = userId});
        }

        foreach (var userId in msg.requestedTo)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                {action = FriendshipAction.REQUESTED_TO, userId = userId});
        }
    }

    // called by kernel
    [UsedImplicitly]
    public void AddFriendsWithDirectMessages(string json)
    {
        var friendsWithDMs = JsonUtility.FromJson<AddFriendsWithDirectMessagesPayload>(json);
        TotalFriendsWithDirectMessagesCount = friendsWithDMs.totalFriendsWithDirectMessages;
        
        AddFriends(friendsWithDMs.currentFriendsWithDirectMessages.Select(messages => messages.userId));

        OnAddFriendsWithDirectMessages?.Invoke(friendsWithDMs.currentFriendsWithDirectMessages.ToList());
    }

    // called by kernel
    [UsedImplicitly]
    public void UpdateUserPresence(string json)
    {
        UserStatus newUserStatus = JsonUtility.FromJson<UserStatus>(json);

        if (!friends.ContainsKey(newUserStatus.userId))
            return;

        // Kernel doesn't send the friendship status on this call, we have to keep it or it gets defaulted
        newUserStatus.friendshipStatus = friends[newUserStatus.userId].friendshipStatus;

        UpdateUserStatus(newUserStatus);
    }
    
    // called by kernel
    [UsedImplicitly]
    public void UpdateFriendshipStatus(string json)
    {
        FriendshipUpdateStatusMessage msg = JsonUtility.FromJson<FriendshipUpdateStatusMessage>(json);
        UpdateFriendshipStatus(msg);
    }

    // called by kernel
    [UsedImplicitly]
    public void UpdateTotalFriendRequests(string json)
    {
        var msg = JsonUtility.FromJson<UpdateTotalFriendRequestsPayload>(json);
        TotalReceivedFriendRequestCount = msg.totalReceivedRequests;
        TotalSentFriendRequestCount = msg.totalSentRequests;
        OnTotalFriendRequestUpdated?.Invoke(TotalReceivedFriendRequestCount, TotalSentFriendRequestCount);
    }

    // called by kernel
    [UsedImplicitly]
    public void UpdateTotalFriends(string json)
    {
        var msg = JsonUtility.FromJson<UpdateTotalFriendsPayload>(json);
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

        if (friendshipStatus == FriendshipStatus.FRIEND)
            friends[userId].friendshipStartedTime = DateTime.UtcNow;

        if (VERBOSE)
            Debug.Log($"Change friend status of {userId} to {friends[userId].friendshipStatus}");

        if (friendshipStatus == FriendshipStatus.NOT_FRIEND)
            friends.Remove(userId);

        OnUpdateFriendship?.Invoke(userId, msg.action);
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
    
    private void AddFriends(IEnumerable<string> friendIds)
    {
        foreach (var friendId in friendIds)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage
                {action = FriendshipAction.APPROVED, userId = friendId});
        }
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

    [ContextMenu("Force initialization")]
    public void ForceInitialization()
    {
        InitializeFriends(JsonUtility.ToJson(new FriendshipInitializationMessage()));
    }
}