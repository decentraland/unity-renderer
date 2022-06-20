using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class LazyLoadingFriendsControllerMock : IFriendsController
{
    private readonly FriendsController controller;
    private readonly UserProfileController userProfileController;

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

    public LazyLoadingFriendsControllerMock(FriendsController controller,
        UserProfileController userProfileController)
    {
        this.controller = controller;
        this.userProfileController = userProfileController;
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
        GetFakeFriendsAsync(limit, skip, "ff").Forget();
    }

    public void GetFriendsAsync(string usernameOrId)
    {
        GetFakeFriendsAsync(30, 0, usernameOrId).Forget();
    }

    public void GetFriendRequestsAsync(
        int sentLimit,
        long sentFromTimestamp,
        int receivedLimit,
        long receivedFromTimestamp)
    {
        GetFakeRequestsAsync(sentLimit, sentFromTimestamp).Forget();
    }

    public void GetFriendsWithDirectMessages(
        int limit,
        long fromTimestamp)
    {
        SimulateDelayedResponseFor_GetFriendsWithDirectMessages(limit, "ffwdm").Forget();
    }

    public void GetFriendsWithDirectMessages(string userNameOrId, int limit)
    {
        SimulateDelayedResponseFor_GetFriendsWithDirectMessages(limit, userNameOrId).Forget();
    }

    private async UniTask SimulateDelayedResponseFor_GetFriendsWithDirectMessages(int limit, string name)
    {
        await UniTask.Delay(Random.Range(1000, 3000));

        controller.AddFriendsWithDirectMessages(
            CreateMockedDataFor_AddFriendsWithDirectMessagesPayload(limit, name));
    }

    private string CreateMockedDataFor_AddFriendsWithDirectMessagesPayload(int numberOfUsers, string name)
    {
        AddFriendsWithDirectMessagesPayload mockedPayload = new AddFriendsWithDirectMessagesPayload();
        List<FriendWithDirectMessages> mockedFriendWithDirectMessages = new List<FriendWithDirectMessages>();

        for (int i = 0; i < numberOfUsers; i++)
        {
            string fakeUserId = $"fakeuser-{name}-{i + 1}";

            mockedFriendWithDirectMessages.Add(
                new FriendWithDirectMessages
                {
                    userId = fakeUserId,
                    lastMessageBody = $"This is the last message sent for {fakeUserId}",
                    lastMessageTimestamp = DateTimeOffset.UtcNow.AddDays(Random.Range(-10, 0))
                        .ToUnixTimeMilliseconds()
                });

            CreateFakeFriend(fakeUserId);
        }

        mockedPayload.currentFriendsWithDirectMessages = mockedFriendWithDirectMessages.ToArray();

        return JsonUtility.ToJson(mockedPayload);
    }
    
    private async UniTask GetFakeFriendsAsync(int limit, int skip, string name)
    {
        var friendIds = new List<string>();
        
        await UniTask.Delay(Random.Range(20, 500));
        
        // fake no more friends to load case
        if (skip > 0 && Random.Range(0, 3) == 0) return;
        
        var characters = new[]
            {'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        
        for (var i = skip; i < skip + Random.Range(1, limit); i++)
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
            currentFriends = friendIds.ToArray()
        };
        
        controller.AddFriends(JsonUtility.ToJson(payload));
    }
    
    private async UniTask GetFakeRequestsAsync(int limit, long timestamp)
    {
        var characters = new[]
            {'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        
        await UniTask.Delay(Random.Range(20, 500));
        
        // fake no more requests to load case
        if (timestamp > 0 && Random.Range(0, 3) == 0) return;
        
        var fromUserIds = new List<string>();
        
        for (var i = 0; i < Random.Range(1, limit); i++)
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
        
        for (var i = 0; i < Random.Range(1, limit); i++)
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
            requestedTo = toUserIds.ToArray()
        };
        
        controller.AddFriendRequests(JsonUtility.ToJson(payload));
    }

    private void CreateFakeFriend(string userId)
    {
        controller.friends.Add(userId, new FriendsController.UserStatus
        {
            userId = userId,
            position = new Vector2(Random.Range(-100, 101), Random.Range(-100, 101)),
            realm = new FriendsController.UserStatus.Realm
            {
                serverName = "dg",
                layer = ""
            },
            presence = Random.Range(0, 2) == 0 ? PresenceStatus.OFFLINE : PresenceStatus.ONLINE,
            friendshipStatus = FriendshipStatus.FRIEND,
            friendshipStartedTime = DateTime.UtcNow
        });
    }
}