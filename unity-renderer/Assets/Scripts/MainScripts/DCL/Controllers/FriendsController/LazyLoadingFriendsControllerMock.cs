using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FriendsController;

public class LazyLoadingFriendsControllerMock : IFriendsController
{
    private const int MAX_AMOUNT_OF_FAKE_FRIENDS = 130;

    private FriendsController controller;
    private int amountOfFriendsWithDirectMessagesRequested = 0;
    private int lastFriendWithDirectMessageIndex = -1;

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

    public event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages
    {
        add => controller.OnAddFriendsWithDirectMessages += value;
        remove => controller.OnAddFriendsWithDirectMessages -= value;
    }

    public int FriendCount => controller.FriendCount;
    public bool IsInitialized => controller.IsInitialized;
    public int ReceivedRequestCount => controller.ReceivedRequestCount;
    public int TotalFriendRequestCount => controller.TotalFriendRequestCount;

    public LazyLoadingFriendsControllerMock(FriendsController controller)
    {
        this.controller = controller;

        // TODO: Use it when the friends service is down
        SimulateDelayedResponseFor_InitializeFriends();
    }

    public Dictionary<string, FriendsController.UserStatus> GetAllocatedFriends() => controller.GetAllocatedFriends();

    public FriendsController.UserStatus GetUserStatus(string userId) => controller.GetUserStatus(userId);

    public bool ContainsStatus(string friendId, FriendshipStatus status) => controller.ContainsStatus(friendId, status);

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
        int sentLimit,
        long sentFromTimestamp,
        int receivedLimit,
        long receivedFromTimestamp)
    {
        // TODO:
        // 1. Prepare a set of fake data
        // 2. Delay
        // 3. Simulate the kernel response (call to the corresponding controller method that manage the response)
    }

    public void GetFriendsWithDirectMessages(
        int limit,
        long fromTimestamp)
    {
        SimulateDelayedResponseFor_GetFriendsWithDirectMessages(limit);
    }

    public void GetFriendsWithDirectMessages(
        string userNameOrId,
        int limit)
    {
        SimulateDelayedResponseFor_GetFriendsWithDirectMessages(userNameOrId, limit);
    }

    private async UniTask SimulateDelayedResponseFor_GetFriendsWithDirectMessages(int limit)
    {
        await UniTask.Delay(UnityEngine.Random.Range(1000, 3000));

        controller.AddFriendsWithDirectMessages(
            CreateMockedDataFor_AddFriendsWithDirectMessagesPayload(limit));
    }

    private string CreateMockedDataFor_AddFriendsWithDirectMessagesPayload(int numberOfUsers)
    {
        AddFriendsWithDirectMessagesPayload mockedPayload = new AddFriendsWithDirectMessagesPayload();
        List<FriendWithDirectMessages> mockedFriendWithDirectMessages = new List<FriendWithDirectMessages>();

        int indexToStart = lastFriendWithDirectMessageIndex + 1;

        for (int i = indexToStart; i < indexToStart + numberOfUsers; i++)
        {
            if (amountOfFriendsWithDirectMessagesRequested >= MAX_AMOUNT_OF_FAKE_FRIENDS)
                break;

            string fakeUserId = $"fakeuser{i + 1}";

            mockedFriendWithDirectMessages.Add(
                new FriendWithDirectMessages
                {
                    userId = fakeUserId,
                    lastMessageBody = $"This is the last message sent for {fakeUserId}",
                    lastMessageTimestamp = DateTimeOffset.UtcNow.AddMinutes(-(i + 1)).ToUnixTimeMilliseconds()
                });

            amountOfFriendsWithDirectMessagesRequested++;
            lastFriendWithDirectMessageIndex = i;

            CreateFakeFriend(fakeUserId);
        }

        mockedPayload.currentFriendsWithDirectMessages = mockedFriendWithDirectMessages.ToArray();
        mockedPayload.totalFriendsWithDirectMessages = MAX_AMOUNT_OF_FAKE_FRIENDS;


        return JsonUtility.ToJson(mockedPayload);
    }

    private async UniTask SimulateDelayedResponseFor_GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
        await UniTask.Delay(UnityEngine.Random.Range(1000, 3000));

        controller.AddFriendsWithDirectMessages(
            CreateMockedDataFor_AddFriendsWithDirectMessagesPayload(userNameOrId, limit));
    }

    private string CreateMockedDataFor_AddFriendsWithDirectMessagesPayload(string userNameOrId, int numberOfUsers)
    {
        List<string> allFriendsWithDMsInServer = new List<string>();
        for (int i = 0; i < MAX_AMOUNT_OF_FAKE_FRIENDS; i++)
        {
            allFriendsWithDMsInServer.Add($"fakeuser{i + 1}");
        }

        List<string> resultsFound = allFriendsWithDMsInServer.Where(x => x.ToLower().Contains(userNameOrId.ToLower())).ToList();

        AddFriendsWithDirectMessagesPayload mockedPayload = new AddFriendsWithDirectMessagesPayload();
        List<FriendWithDirectMessages> mockedFriendWithDirectMessages = new List<FriendWithDirectMessages>();

        for (int i = 0; i < resultsFound.Count(); i++)
        {
            if (i >= numberOfUsers)
                break;

            mockedFriendWithDirectMessages.Add(
                new FriendWithDirectMessages
                {
                    userId = resultsFound[i],
                    lastMessageBody = $"This is the last message sent for {resultsFound[i]}",
                    lastMessageTimestamp = DateTimeOffset.UtcNow.AddMinutes(-(i + 1)).ToUnixTimeMilliseconds()
                });

            CreateFakeFriend(resultsFound[i]);
        }

        mockedPayload.currentFriendsWithDirectMessages = mockedFriendWithDirectMessages.ToArray();
        mockedPayload.totalFriendsWithDirectMessages = MAX_AMOUNT_OF_FAKE_FRIENDS;

        return JsonUtility.ToJson(mockedPayload);
    }

    private void CreateFakeFriend(string userId)
    {
        if (controller.friends.ContainsKey(userId))
            return;

        controller.friends.Add(userId, new FriendsController.UserStatus
        {
            userId = userId,
            position = new Vector2(UnityEngine.Random.Range(-100, 101), UnityEngine.Random.Range(-100, 101)),
            realm = new FriendsController.UserStatus.Realm
            {
                serverName = "dg",
                layer = ""
            },
            presence = UnityEngine.Random.Range(0, 2) == 0 ? PresenceStatus.OFFLINE : PresenceStatus.ONLINE,
            friendshipStatus = FriendshipStatus.FRIEND,
            friendshipStartedTime = DateTime.UtcNow
        });
    }

    private async UniTask SimulateDelayedResponseFor_InitializeFriends()
    {
        await UniTask.Delay(UnityEngine.Random.Range(1000, 3000));

        controller.InitializeFriends(
            CreateFakeFriendsInitialization());
    }

    private string CreateFakeFriendsInitialization()
    {
        FriendshipInitializationMessage mockedPayload = new FriendshipInitializationMessage();
        return JsonUtility.ToJson(mockedPayload);
    }
}