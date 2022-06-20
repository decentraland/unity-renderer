using System;
using System.Collections.Generic;

public interface IFriendsController
{
    event Action OnInitialized;
    event Action<string, FriendshipAction> OnUpdateFriendship;
    event Action<string, FriendsController.UserStatus> OnUpdateUserStatus;
    event Action<string> OnFriendNotFound;
    event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;

    int FriendCount { get; }
    bool IsInitialized { get; }
    int ReceivedRequestCount { get; }
    Dictionary<string, FriendsController.UserStatus> GetAllocatedFriends();
    FriendsController.UserStatus GetUserStatus(string userId);

    bool ContainsStatus(string friendId, FriendshipStatus status);
    void RequestFriendship(string friendUserId);
    void CancelRequest(string friendUserId);
    void AcceptFriendship(string friendUserId);
    void RejectFriendship(string friendUserId);
    bool IsFriend(string userId);
    void RemoveFriend(string friendId);
    void GetFriendsAsync(int limit, int skip);
    void GetFriendsAsync(string usernameOrId);
    void GetFriendRequestsAsync(int sentLimit, long sentFromTimestamp, int receivedLimit, long receivedFromTimestamp);
    void GetFriendsWithDirectMessages(int limit, long fromTimestamp);
    void GetFriendsWithDirectMessages(string userNameOrId, int limit);
}