using System;
using System.Collections.Generic;
using System.Linq;

public class FriendsController_Mock : IFriendsController
{
    public event Action<string, FriendshipAction> OnUpdateFriendship;
    public event Action<string, FriendsController.UserStatus> OnUpdateUserStatus;
    public event Action<string> OnFriendNotFound;
    public event Action OnInitialized;

    private readonly Dictionary<string, FriendsController.UserStatus> friends = new Dictionary<string, FriendsController.UserStatus>();

    public int FriendCount => friends.Count;

    public bool IsInitialized => true;

    public int ReceivedRequestCount =>
        friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);

    public Dictionary<string, FriendsController.UserStatus> GetAllocatedFriends() { return friends; }
    
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

    public void GetFriendsAsync(int limit, int skip)
    {
    }

    public void GetFriendsAsync(string usernameOrId)
    {
    }

    public void GetFriendRequestsAsync(int sentLimit, long sentFromTimestamp, int receivedLimit, long receivedFromTimestamp)
    {
        throw new NotImplementedException();
    }

    public void GetFriendsWithDirectMessages(int limit, long fromTimestamp)
    {
        throw new NotImplementedException();
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
        throw new NotImplementedException();
    }

    public FriendsController.UserStatus GetUserStatus(string userId)
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
            friends.Add(friendUserId, new FriendsController.UserStatus{friendshipStatus = FriendshipStatus.REQUESTED_TO});
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
                friends.Add(id, new FriendsController.UserStatus());
        }

        OnUpdateFriendship?.Invoke(id, action);
    }

    public void RaiseUpdateUserStatus(string id, FriendsController.UserStatus userStatus) { OnUpdateUserStatus?.Invoke(id, userStatus); }

    public void RaiseOnFriendNotFound(string id) { OnFriendNotFound?.Invoke(id); }

    public void AddFriend(FriendsController.UserStatus newFriend) { friends.Add(newFriend.userId, newFriend); }
}