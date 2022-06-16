using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class LazyLoadingFriendsControllerMock : IFriendsController
{
    private FriendsController controller;

    public event Action OnInitialized
    {
        add => controller.OnInitialized += value;
        remove => controller.OnInitialized -= value;
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

    public event Action<List<string>> OnAddFriendsWithDirectMessages
    {
        add => controller.OnAddFriendsWithDirectMessages += value;
        remove => controller.OnAddFriendsWithDirectMessages -= value;
    }

    public int FriendCount => controller.FriendCount;
    public bool IsInitialized => controller.IsInitialized;
    public int ReceivedRequestCount => controller.ReceivedRequestCount;

    public LazyLoadingFriendsControllerMock(FriendsController controller)
    {
        this.controller = controller;
    }

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
        // TODO:
        // 1. Prepare a set of fake data
        // 2. Delay
        // 3. Simulate the kernel response (call to the corresponding controller method that manage the response)
    }

    public void GetFriendsAsync(string usernameOrId)
    {
        // TODO:
        // 1. Prepare a set of fake data
        // 2. Delay
        // 3. Simulate the kernel response (call to the corresponding controller method that manage the response)
    }

    public void GetFriendRequestsAsync(
        int sentLimit, long sentFromTimestamp,
        int receivedLimit, long receivedFromTimestamp)
    {
        // TODO:
        // 1. Prepare a set of fake data
        // 2. Delay
        // 3. Simulate the kernel response (call to the corresponding controller method that manage the response)
    }

    public void GetFriendsWithDirectMessages(int limit, long fromTimestamp)
    {
        SimulateDelayedResponseFor_GetFriendsWithDirectMessages();
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
        // TODO:
        // 1. Prepare a set of fake data
        // 2. Delay
        // 3. Simulate the kernel response (call to the corresponding controller method that manage the response)
    }

    private async UniTask SimulateDelayedResponseFor_GetFriendsWithDirectMessages()
    {
        await UniTask.Delay(3000);
        controller.AddFriendsWithDirectMessages(
            CreateMockedDataFor_AddFriendsWithDirectMessagesPayload());
    }

    private string CreateMockedDataFor_AddFriendsWithDirectMessagesPayload()
    {
        string mockedJson = "{ \"currentFriendsWithDirectMessages\": [";

        for (int i = 0; i < 10; i++)
            mockedJson += $"\"fakeuser{i + 1}\",";

        mockedJson = mockedJson.Remove(mockedJson.Length - 1) + "]}";

        return mockedJson;
    }
}