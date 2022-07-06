using System;
using System.Collections.Generic;
using DCL.Friends.WebApi;

public interface IFriendsController
{
    event Action OnInitialized;
    event Action<string, FriendshipAction> OnUpdateFriendship;
    event Action<string, UserStatus> OnUpdateUserStatus;
    event Action<string> OnFriendNotFound;
    event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
    event Action<int, int> OnTotalFriendRequestUpdated;
    event Action<int> OnTotalFriendsUpdated;

    int AllocatedFriendCount { get; }
    bool IsInitialized { get; }
    int ReceivedRequestCount { get; }
    int TotalFriendCount { get; }
    int TotalFriendRequestCount { get; }
    int TotalReceivedFriendRequestCount { get; }
    int TotalSentFriendRequestCount { get; }
    int TotalFriendsWithDirectMessagesCount { get; }
    Dictionary<string, UserStatus> GetAllocatedFriends();
    UserStatus GetUserStatus(string userId);

    bool ContainsStatus(string friendId, FriendshipStatus status);
    void RequestFriendship(string friendUserId);
    void CancelRequest(string friendUserId);
    void AcceptFriendship(string friendUserId);
    void RejectFriendship(string friendUserId);
    bool IsFriend(string userId);
    void RemoveFriend(string friendId);
    void GetFriendsAsync(int limit, int skip);
    void GetFriendsAsync(string usernameOrId, int limit);
    void GetFriendRequestsAsync(int sentLimit, long sentFromTimestamp, int receivedLimit, long receivedFromTimestamp);
    void GetFriendsWithDirectMessages(int limit, long fromTimestamp);
    void GetFriendsWithDirectMessages(string userNameOrId, int limit);
}