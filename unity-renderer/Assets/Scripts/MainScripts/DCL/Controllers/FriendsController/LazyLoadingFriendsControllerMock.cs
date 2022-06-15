using System;
using System.Collections.Generic;

public class LazyLoadingFriendsControllerMock : IFriendsController
{
    private FriendsController controller;

    public event Action OnInitialized;

    public LazyLoadingFriendsControllerMock(FriendsController controller)
    {
        this.controller = controller;
    }

    public event Action<string, FriendshipAction> OnUpdateFriendship
    {
        add => controller.OnUpdateFriendship += value;
        remove => controller.OnUpdateFriendship -= value;
    }
    
    public event Action<string, FriendsController.UserStatus> OnUpdateUserStatus
    {
        add => controller.OnUpdateUserStatus += value;
        remove => controller.OnUpdateUserStatus -= value;
    }
    
    public event Action<string> OnFriendNotFound
    {
        add => controller.OnFriendNotFound += value;
        remove => controller.OnFriendNotFound -= value;
    }

    public int FriendCount => controller.FriendCount;
    public bool IsInitialized => controller.IsInitialized;
    public int ReceivedRequestCount => controller.ReceivedRequestCount;

    public Dictionary<string, FriendsController.UserStatus> GetAllocatedFriends() => controller.GetAllocatedFriends();

    public FriendsController.UserStatus GetUserStatus(string userId) => controller.GetUserStatus(userId);

    public bool ContainsStatus(string friendId, FriendshipStatus status) =>
        controller.ContainsStatus(friendId, status);

    public void RequestFriendship(string friendUserId) => controller.RequestFriendship(friendUserId);

    public void CancelRequest(string friendUserId) => controller.CancelRequest(friendUserId);

    public void AcceptFriendship(string friendUserId) => controller.AcceptFriendship(friendUserId);

    public void RejectFriendship(string friendUserId) => controller.RejectFriendship(friendUserId);

    public bool IsFriend(string userId) => controller.IsFriend(userId);

    public void RemoveFriend(string friendId) => controller.RemoveFriend(friendId);
    
    public void GetFriendsAsync(int limit, int skip)
    {
        //await UniTask.Delay(Random.Range(100, 700));
        // TODO: fake users
        //return new Dictionary<string, FriendsController.UserStatus>();
    }

    public void GetFriendsAsync(string usernameOrId)
    {
        //await UniTask.Delay(Random.Range(100, 700));
        // TODO: fake users
        //return new Dictionary<string, FriendsController.UserStatus>();
    }

    public void GetFriendRequestsAsync(
        int sentLimit, long sentFromTimestamp,
        int receivedLimit, long receivedFromTimestamp)
    {
        //await UniTask.Delay(Random.Range(100, 700));
        // TODO: fake requests
        //return new Dictionary<string, FriendsController.UserStatus>();
    }

    public void GetFriendsWithDirectMessages(int limit, long fromTimestamp)
    {
        //await UniTask.Delay(Random.Range(100, 700));
        // TODO: fake users
        //return new Dictionary<string, FriendsController.UserStatus>();
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
        //await UniTask.Delay(Random.Range(100, 700));
        // TODO: fake users
        //return new Dictionary<string, FriendsController.UserStatus>();
    }
}