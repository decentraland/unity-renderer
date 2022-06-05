using System;
using System.Collections.Generic;

public interface IFriendsController
{
    event Action OnInitialized;
    event Action<string, FriendshipAction> OnUpdateFriendship;
    event Action<string, FriendsController.UserStatus> OnUpdateUserStatus;
    event Action<string> OnFriendNotFound;

    int friendCount { get; }
    bool isInitialized { get; }
    int ReceivedRequestCount { get; }
    Dictionary<string, FriendsController.UserStatus> GetFriends();
    FriendsController.UserStatus GetUserStatus(string userId);

    bool ContainsStatus(string friendId, FriendshipStatus status);
    void RequestFriendship(string friendUserId);
    void CancelRequest(string friendUserId);
    void AcceptFriendship(string friendUserId);
    void RejectFriendship(string friendUserId);
    bool IsFriend(string userId);
    void RemoveFriend(string friendId);
}