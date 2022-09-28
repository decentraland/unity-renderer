using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL.Friends.WebApi;
using UnityEngine;
using Random = UnityEngine.Random;

public class LazyLoadingFriendsControllerMock : IFriendsController
{
    private const int MAX_AMOUNT_OF_FAKE_FRIENDS = 130;
    private const int TOTAL_RECEIVED_REQUESTS = 14;
    private const int TOTAL_SENT_REQUESTS = 18;
    private const int TOTAL_FRIENDS = 46;

    private readonly FriendsController controller;
    private readonly UserProfileController userProfileController;

    private int amountOfFriendsWithDirectMessagesRequested;
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

    public event Action<string, UserStatus> OnUpdateUserStatus
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

    public event Action<int, int> OnTotalFriendRequestUpdated
    {
        add => controller.OnTotalFriendRequestUpdated += value;
        remove => controller.OnTotalFriendRequestUpdated -= value;
    }

    public event Action<int> OnTotalFriendsUpdated
    {
        add => controller.OnTotalFriendsUpdated += value;
        remove => controller.OnTotalFriendsUpdated -= value;
    }

    public int AllocatedFriendCount => controller.AllocatedFriendCount;
    public bool IsInitialized => controller.IsInitialized;
    public int ReceivedRequestCount => controller.ReceivedRequestCount;
    public int TotalFriendCount => controller.TotalFriendCount;
    public int TotalFriendRequestCount => controller.TotalFriendRequestCount;
    public int TotalReceivedFriendRequestCount => controller.TotalReceivedFriendRequestCount;
    public int TotalSentFriendRequestCount => controller.TotalSentFriendRequestCount;
    public int TotalFriendsWithDirectMessagesCount => controller.TotalFriendsWithDirectMessagesCount;

    public LazyLoadingFriendsControllerMock(
        FriendsController controller,
        UserProfileController userProfileController)
    {
        this.controller = controller;
        this.userProfileController = userProfileController;

        // TODO: Use it when the friends service is down
        SimulateDelayedResponseFor_InitializeFriends().Forget();
    }

    public Dictionary<string, UserStatus> GetAllocatedFriends() => controller.GetAllocatedFriends();

    public UserStatus GetUserStatus(string userId) => controller.GetUserStatus(userId);

    public bool ContainsStatus(string friendId, FriendshipStatus status) => controller.ContainsStatus(friendId, status);

    public void RequestFriendship(string friendUserId)
    {
        controller.RequestFriendship(friendUserId);
        UpdateFriendshipCount(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100)).Forget();
    }

    public void CancelRequest(string friendUserId)
    {
        controller.CancelRequest(friendUserId);
        UpdateFriendshipCount(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100)).Forget();
    }

    public void AcceptFriendship(string friendUserId)
    {
        controller.AcceptFriendship(friendUserId);
        UpdateFriendshipCount(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100)).Forget();
    }
    
    public void RejectFriendship(string friendUserId)
    {
        controller.RejectFriendship(friendUserId);
        UpdateFriendshipCount(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100)).Forget();
    }

    public bool IsFriend(string userId) => controller.IsFriend(userId);

    public void RemoveFriend(string friendId)
    {
        controller.RemoveFriend(friendId);
        UpdateFriendshipCount(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100)).Forget();
    }

    public void GetFriends(int limit, int skip)
    {
        GetFakeFriendsAsync(limit, skip, "ff").Forget();
    }

    public void GetFriends(string usernameOrId, int limit)
    {
        GetFakeFriendsAsync(limit, 0, usernameOrId).Forget();
    }

    public void GetFriendRequests(
        int sentLimit,
        int sentSkip,
        int receivedLimit,
        int receivedSkip)
    {
        GetFakeRequestsAsync(sentLimit).Forget();
    }

    public void GetFriendsWithDirectMessages(int limit, int skip)
    {
        SimulateDelayedResponseFor_GetFriendsWithDirectMessages(limit).Forget();
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
        SimulateDelayedResponseFor_GetFriendsWithDirectMessages(userNameOrId, limit).Forget();
    }

    private async UniTask SimulateDelayedResponseFor_GetFriendsWithDirectMessages(int limit)
    {
        await UniTask.Delay(Random.Range(1000, 3000));

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
        await UniTask.Delay(Random.Range(1000, 3000));

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

        List<string> resultsFound =
            allFriendsWithDMsInServer.Where(x => x.ToLower().Contains(userNameOrId.ToLower())).ToList();

        AddFriendsWithDirectMessagesPayload mockedPayload = new AddFriendsWithDirectMessagesPayload();
        List<FriendWithDirectMessages> mockedFriendWithDirectMessages = new List<FriendWithDirectMessages>();

        for (int i = 0; i < resultsFound.Count; i++)
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

    private async UniTask GetFakeFriendsAsync(int limit, int skip, string name)
    {
        var friendIds = new List<string>();

        await UniTask.Delay(Random.Range(20, 500));

        var characters = new[]
        {
            'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        var max = Mathf.Min(skip + Random.Range(1, limit), TOTAL_FRIENDS);

        for (var i = skip; i < max; i++)
        {
            var userId = "";
            for (var x = 0; x < 8; x++)
                userId += characters[Random.Range(0, characters.Length)];

            friendIds.Add(userId);

            userProfileController.AddUserProfileToCatalog(JsonUtility.ToJson(new UserProfileModel
            {
                userId = userId,
                name = $"{name}-{i}-{userId}",
                ethAddress = userId,
                snapshots = new UserProfileModel.Snapshots {face256 = $"https://picsum.photos/seed/{i + 1}/256"}
            }));
        }

        await UniTask.Delay(Random.Range(20, 500));

        var payload = new AddFriendsPayload
        {
            friends = friendIds.ToArray(),
            totalFriends = TOTAL_FRIENDS
        };

        controller.AddFriends(JsonUtility.ToJson(payload));
    }

    private async UniTask GetFakeRequestsAsync(int limit)
    {
        var characters = new[]
        {
            'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        await UniTask.Delay(Random.Range(20, 500));

        var fromUserIds = new List<string>();
        var maxReceivedRequests = Mathf.Min(TOTAL_RECEIVED_REQUESTS, Random.Range(1, limit));

        for (var i = 0; i < maxReceivedRequests; i++)
        {
            var userId = "";
            for (var x = 0; x < 8; x++)
                userId += characters[Random.Range(0, characters.Length)];

            fromUserIds.Add(userId);

            userProfileController.AddUserProfileToCatalog(JsonUtility.ToJson(new UserProfileModel
            {
                userId = userId,
                name = $"ffrrq-{i}-{userId}",
                ethAddress = userId,
                snapshots = new UserProfileModel.Snapshots {face256 = $"https://picsum.photos/seed/{i + 1}/256"}
            }));
        }

        var toUserIds = new List<string>();
        var maxSentRequests = Mathf.Min(TOTAL_SENT_REQUESTS, Random.Range(1, limit));

        for (var i = 0; i < maxSentRequests; i++)
        {
            var userId = "";
            for (var x = 0; x < 8; x++)
                userId += characters[Random.Range(0, characters.Length)];

            toUserIds.Add(userId);

            userProfileController.AddUserProfileToCatalog(JsonUtility.ToJson(new UserProfileModel
            {
                userId = userId,
                name = $"ffsrq-{i}-{userId}",
                ethAddress = userId,
                snapshots = new UserProfileModel.Snapshots {face256 = $"https://picsum.photos/seed/{i + 1}/256"}
            }));
        }

        await UniTask.Delay(Random.Range(20, 500));

        var payload = new AddFriendRequestsPayload
        {
            requestedFrom = fromUserIds.ToArray(),
            requestedTo = toUserIds.ToArray(),
            totalReceivedFriendRequests = TOTAL_RECEIVED_REQUESTS,
            totalSentFriendRequests = TOTAL_SENT_REQUESTS
        };

        controller.AddFriendRequests(JsonUtility.ToJson(payload));
    }

    private void CreateFakeFriend(string userId)
    {
        if (controller.friends.ContainsKey(userId))
            return;

        controller.friends.Add(userId, new UserStatus
        {
            userId = userId,
            position = new Vector2(Random.Range(-100, 101), Random.Range(-100, 101)),
            realm = new UserStatus.Realm
            {
                serverName = "dg",
                layer = ""
            },
            presence = Random.Range(0, 2) == 0 ? PresenceStatus.OFFLINE : PresenceStatus.ONLINE,
            friendshipStatus = FriendshipStatus.FRIEND,
            friendshipStartedTime = DateTime.UtcNow
        });
    }

    private async UniTask SimulateDelayedResponseFor_InitializeFriends()
    {
        await UniTask.Delay(Random.Range(1000, 3000));

        controller.InitializeFriends(
            CreateFakeFriendsInitialization());
    }

    private string CreateFakeFriendsInitialization()
    {
        var mockedPayload = new FriendshipInitializationMessage
        {
            totalReceivedRequests = TOTAL_RECEIVED_REQUESTS 
        };
        
        return JsonUtility.ToJson(mockedPayload);
    }
    
    private async UniTask UpdateFriendshipCount(int totalReceivedRequests, int totalSentRequests, int totalFriends)
    {
        await UniTask.Delay(Random.Range(100, 2000));

        var requestsPayload = new UpdateTotalFriendRequestsPayload
        {
            totalReceivedRequests = totalReceivedRequests,
            totalSentRequests = totalSentRequests
        };
        controller.UpdateTotalFriendRequests(JsonUtility.ToJson(requestsPayload));

        var friendsPayload = new UpdateTotalFriendsPayload
        {
            totalFriends = totalFriends
        };
        controller.UpdateTotalFriends(JsonUtility.ToJson(friendsPayload));
    }
}