using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IFriendsController
{
    event Action OnInitialized;
    event Action<string, FriendshipAction> OnUpdateFriendship;
    event Action<string, FriendsController.UserStatus> OnUpdateUserStatus;
    event Action<string> OnFriendNotFound;

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
    UniTask<Dictionary<string, FriendsController.UserStatus>> GetFriendsAsync(int limit, int skip);
    UniTask<Dictionary<string, FriendsController.UserStatus>> GetFriendsAsync(string usernameOrId);

    UniTask<Dictionary<string, FriendsController.UserStatus>> GetFriendRequestsAsync(
        int sentLimit, long sentFromTimestamp,
        int receivedLimit, long receivedFromTimestamp);

    UniTask<Dictionary<string, FriendsController.UserStatus>> GetFriendsWithDirectMessages(int limit, long fromTimestamp);
    UniTask<Dictionary<string, FriendsController.UserStatus>> GetFriendsWithDirectMessages(string userNameOrId);
}