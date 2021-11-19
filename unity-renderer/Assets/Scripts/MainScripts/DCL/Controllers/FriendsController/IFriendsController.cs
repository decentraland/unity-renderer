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
    Dictionary<string, FriendsController.UserStatus> GetFriends();
    FriendsController.UserStatus GetUserStatus(string userId);

    void RequestFriendship(string friendUserId);
    void CancelRequest(string friendUserId);
    void AcceptFriendship(string friendUserId);
    void RejectFriendship(string friendUserId);
}