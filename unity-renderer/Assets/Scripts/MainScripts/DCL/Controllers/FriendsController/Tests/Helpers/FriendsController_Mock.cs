using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Friends.WebApi;

public class FriendsController_Mock : IFriendsController
{
    public event Action<string, FriendshipAction> OnUpdateFriendship;
    public event Action<string, UserStatus> OnUpdateUserStatus;
    public event Action<string> OnFriendNotFound;
    public event Action OnInitialized;
    public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
    public event Action<int, int> OnTotalFriendRequestUpdated;
    public event Action<int> OnTotalFriendsUpdated;

    private readonly Dictionary<string, UserStatus> friends = new Dictionary<string, UserStatus>();

    public int AllocatedFriendCount => friends.Count;

    public bool IsInitialized => true;

    public int ReceivedRequestCount =>
        friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);

    public int TotalFriendCount { get; }
    public int TotalFriendRequestCount { get; }
    public int TotalReceivedFriendRequestCount { get; }
    public int TotalSentFriendRequestCount { get; }
    public int TotalFriendsWithDirectMessagesCount => friends.Count;

    public Dictionary<string, UserStatus> GetAllocatedFriends() { return friends; }
    
    public void RejectFriendship(string friendUserId)
    {
        friends.Remove(friendUserId);
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.NONE);
    }

    public bool IsFriend(string userId) => friends.ContainsKey(userId);
    
    public void RemoveFriend(string friendId)
    {
        if (!friends.ContainsKey(friendId)) return;
        friends.Remove(friendId);
        OnUpdateFriendship?.Invoke(friendId, FriendshipAction.DELETED);
    }

    public void GetFriends(int limit, int skip)
    {
    }

    public void GetFriends(string usernameOrId, int limit)
    {
    }

    public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
    {
    }

    public void GetFriendsWithDirectMessages(int limit, int skip)
    {
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
    }

    public UserStatus GetUserStatus(string userId)
    {
        return friends.ContainsKey(userId) ? friends[userId] : default;
    }

    public bool ContainsStatus(string friendId, FriendshipStatus status)
    {
        return friends.ContainsKey(friendId) && friends[friendId].friendshipStatus == status;
    }

    public void RequestFriendship(string friendUserId)
    {
        if (!friends.ContainsKey(friendUserId))
            friends.Add(friendUserId, new UserStatus{friendshipStatus = FriendshipStatus.REQUESTED_TO});
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.REQUESTED_TO);
    }

    public void CancelRequest(string friendUserId)
    {
        if (!friends.ContainsKey(friendUserId)) return;
        friends.Remove(friendUserId);
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.CANCELLED);
    }

    public void AcceptFriendship(string friendUserId)
    {
        if (!friends.ContainsKey(friendUserId)) return;
        friends[friendUserId].friendshipStatus = FriendshipStatus.FRIEND;
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.APPROVED);
    }

    public void RaiseUpdateFriendship(string id, FriendshipAction action)
    {
        if (action == FriendshipAction.NONE)
        {
            if (friends.ContainsKey(id))
                friends.Remove(id);
        }

        if (action == FriendshipAction.APPROVED)
        {
            if (!friends.ContainsKey(id))
                friends.Add(id, new UserStatus());
        }

        OnUpdateFriendship?.Invoke(id, action);
    }

    public void RaiseUpdateUserStatus(string id, UserStatus userStatus) { OnUpdateUserStatus?.Invoke(id, userStatus); }

    public void AddFriend(UserStatus newFriend) { friends.Add(newFriend.userId, newFriend); }
}