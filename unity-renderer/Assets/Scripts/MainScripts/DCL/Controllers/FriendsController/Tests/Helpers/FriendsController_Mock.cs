using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using DCL.Social.Friends;

public class FriendsController_Mock : IFriendsController
{
    public event Action<string, FriendshipAction> OnUpdateFriendship;
    public event Action<string, UserStatus> OnUpdateUserStatus;
    public event Action<string> OnFriendNotFound;
    public event Action OnInitialized;
    public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
    public event Action<int, int> OnTotalFriendRequestUpdated;
    public event Action<int> OnTotalFriendsUpdated;
    public event Action<FriendRequest> OnAddFriendRequest;

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

    public UniTask<List<FriendRequest>> GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
    {
        return UniTask.FromResult(new List<FriendRequest>());
    }

    public void GetFriendsWithDirectMessages(int limit, int skip)
    {
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
    }

    public FriendRequest GetAllocatedFriendRequest(string friendRequestId) =>
        null;

    public FriendRequest GetAllocatedFriendRequestByUser(string userId) =>
        null;

    public UserStatus GetUserStatus(string userId)
    {
        return friends.ContainsKey(userId) ? friends[userId] : default;
    }

    public bool ContainsStatus(string friendId, FriendshipStatus status)
    {
        return friends.ContainsKey(friendId) && friends[friendId].friendshipStatus == status;
    }

    public UniTask<FriendRequest> RequestFriendship(string friendUserId, string messageBody)
    {
        if (!friends.ContainsKey(friendUserId))
            friends.Add(friendUserId, new UserStatus{friendshipStatus = FriendshipStatus.REQUESTED_TO});
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.REQUESTED_TO);
        return UniTask.FromResult(new FriendRequest("oiqwdjqowi", 0, "me", friendUserId, messageBody));
    }

    public async UniTask<FriendRequest> CancelRequestByUserId(string friendUserId)
    {
        if (!friends.ContainsKey(friendUserId)) return null;
        friends.Remove(friendUserId);
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.CANCELLED);
        return new FriendRequest(friendUserId, 0, "", "", "");
    }

    public UniTask<FriendRequest> CancelRequest(string friendRequestId) =>
        UniTask.FromResult(new FriendRequest(friendRequestId, 0, "", "", ""));

    public void AcceptFriendship(string friendUserId)
    {
        if (!friends.ContainsKey(friendUserId)) return;
        friends[friendUserId].friendshipStatus = FriendshipStatus.FRIEND;
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.APPROVED);
    }

    public void RaiseUpdateUserStatus(string id, UserStatus userStatus) { OnUpdateUserStatus?.Invoke(id, userStatus); }

    public void AddFriend(UserStatus newFriend) { friends.Add(newFriend.userId, newFriend); }
}
