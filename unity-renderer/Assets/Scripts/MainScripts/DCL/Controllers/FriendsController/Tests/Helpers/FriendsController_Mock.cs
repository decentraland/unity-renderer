using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using DCL.Social.Friends;
using System.Threading;

public class FriendsController_Mock : IFriendsController
{
    public event Action<string, FriendshipAction> OnUpdateFriendship;
    public event Action<string, UserStatus> OnUpdateUserStatus;
    public event Action<string> OnFriendNotFound;
    public event Action OnInitialized;
    public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
    public event Action<int, int> OnTotalFriendRequestUpdated;
    public event Action<int> OnTotalFriendsUpdated;
    public event Action<FriendRequest> OnFriendRequestReceived;
    public event Action<FriendRequest> OnSentFriendRequestApproved;

    private readonly Dictionary<string, UserStatus> friends = new ();

    public int AllocatedFriendCount => friends.Count;

    public bool IsInitialized => true;

    public int ReceivedRequestCount => friends.Values.Count(status => status.friendshipStatus == FriendshipStatus.REQUESTED_FROM);

    public int TotalFriendCount { get; }
    public int TotalFriendRequestCount { get; }
    public int TotalReceivedFriendRequestCount { get; }
    public int TotalSentFriendRequestCount { get; }
    public int TotalFriendsWithDirectMessagesCount => friends.Count;

    public Dictionary<string, UserStatus> GetAllocatedFriends()
    {
        return friends;
    }

    public UniTask<FriendRequest> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken) =>
        UniTask.FromResult(new FriendRequest(friendRequestId, 0, "", "", ""));

    public void RejectFriendship(string friendUserId)
    {
        friends.Remove(friendUserId);
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.NONE);
    }

    public UniTask<FriendRequest> RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken) =>
        UniTask.FromResult(new FriendRequest(friendRequestId, 0, "", "", ""));

    public bool IsFriend(string userId) =>
        friends.ContainsKey(userId);

    public void RemoveFriend(string friendId)
    {
        if (!friends.ContainsKey(friendId)) return;
        friends.Remove(friendId);
        OnUpdateFriendship?.Invoke(friendId, FriendshipAction.DELETED);
    }

    public UniTask<string[]> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default) =>
        UniTask.FromResult(new string[0]);

    public UniTask<IReadOnlyList<string>> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default) =>
        UniTask.FromResult((IReadOnlyList<string>)Array.Empty<string>());

    public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) { }

    public UniTask<IReadOnlyList<FriendRequest>> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip,
        CancellationToken cancellationToken)
    {
        return UniTask.FromResult((IReadOnlyList<FriendRequest>)new List<FriendRequest>());
    }

    public void GetFriendsWithDirectMessages(int limit, int skip) { }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit) { }

    public FriendRequest GetAllocatedFriendRequest(string friendRequestId) =>
        null;

    public FriendRequest GetAllocatedFriendRequestByUser(string userId) =>
        null;

    public UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken) =>
        UniTask.FromResult(FriendshipStatus.NOT_FRIEND);

    public UserStatus GetUserStatus(string userId)
    {
        return friends.ContainsKey(userId) ? friends[userId] : default;
    }

    public bool ContainsStatus(string friendId, FriendshipStatus status)
    {
        return friends.ContainsKey(friendId) && friends[friendId].friendshipStatus == status;
    }

    public UniTask<FriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody, CancellationToken cancellationToken)
    {
        if (!friends.ContainsKey(friendUserId))
            friends.Add(friendUserId, new UserStatus { friendshipStatus = FriendshipStatus.REQUESTED_TO });

        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.REQUESTED_TO);
        return UniTask.FromResult(new FriendRequest("oiqwdjqowi", 0, "me", friendUserId, messageBody));
    }

    public void RequestFriendship(string friendUserId) { }

    public async UniTask<FriendRequest> CancelRequestByUserIdAsync(string friendUserId, CancellationToken cancellationToken)
    {
        if (!friends.ContainsKey(friendUserId)) return null;
        friends.Remove(friendUserId);
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.CANCELLED);
        return new FriendRequest(friendUserId, 0, "", "", "");
    }

    public void CancelRequestByUserId(string friendUserId) { }

    public UniTask<FriendRequest> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken) =>
        UniTask.FromResult(new FriendRequest(friendRequestId, 0, "", "", ""));

    public void AcceptFriendship(string friendUserId)
    {
        if (!friends.ContainsKey(friendUserId)) return;
        friends[friendUserId].friendshipStatus = FriendshipStatus.FRIEND;
        OnUpdateFriendship?.Invoke(friendUserId, FriendshipAction.APPROVED);
    }

    public void RaiseUpdateUserStatus(string id, UserStatus userStatus)
    {
        OnUpdateUserStatus?.Invoke(id, userStatus);
    }

    public void AddFriend(UserStatus newFriend)
    {
        friends.Add(newFriend.userId, newFriend);
    }

    public void Dispose() { }

    public void Initialize() { }
}
