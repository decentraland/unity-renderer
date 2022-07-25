using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL.Chat.WebApi;
using DCL.Interface;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;
using Random = UnityEngine.Random;

public class LazyLoadingChatControllerMock : IChatController
{
    private const int MAX_AMOUNT_OF_FAKE_USERS_IN_CATALOG = 130;

    private readonly ChatController controller;

    public int TotalUnseenMessages => controller.TotalUnseenMessages;

    public int TotalJoinedChannelCount => 0;

    public event Action<ChatMessage> OnAddMessage
    {
        add => controller.OnAddMessage += value;
        remove => controller.OnAddMessage -= value;
    }

    public event Action<Channel> OnChannelUpdated;
    public event Action<Channel> OnChannelJoined;
    public event Action<string, string> OnJoinChannelError;
    public event Action<string> OnChannelLeft;
    public event Action<string, string> OnChannelLeaveError;
    public event Action<string, string> OnMuteChannelError;
    
    public event Action<int> OnTotalUnseenMessagesUpdated
    {
        add => controller.OnTotalUnseenMessagesUpdated += value;
        remove => controller.OnTotalUnseenMessagesUpdated -= value;
    }
    
    public event Action<string, int> OnUserUnseenMessagesUpdated
    {
        add => controller.OnUserUnseenMessagesUpdated += value;
        remove => controller.OnUserUnseenMessagesUpdated -= value;
    }

    public event Action<string, int> OnChannelUnseenMessagesUpdated
    {
        add => controller.OnChannelUnseenMessagesUpdated += value;
        remove => controller.OnChannelUnseenMessagesUpdated -= value;
    }

    public LazyLoadingChatControllerMock(ChatController controller)
    {
        this.controller = controller;

        CreateFakeUsersInCatalog();
        SimulateDelayedResponseFor_ChatInitialization().Forget();
    }

    public List<ChatMessage> GetAllocatedEntries() => controller.GetAllocatedEntries();

    public void Send(ChatMessage message) => controller.Send(message);

    public void MarkMessagesAsSeen(string userId)
    {
        controller.MarkMessagesAsSeen(userId);

        SimulateDelayedResponseFor_MarkAsSeen(userId).Forget();
    }

    public void MarkChannelMessagesAsSeen(string channelId) => controller.MarkChannelMessagesAsSeen(channelId);

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp) =>
        SimulateDelayedResponseFor_GetPrivateMessages(userId, limit, fromTimestamp).Forget();
    
    public void GetUnseenMessagesByUser() => SimulateDelayedResponseFor_TotalUnseenMessagesByUser().Forget();

    public void GetUnseenMessagesByChannel() => controller.GetUnseenMessagesByChannel();

    public int GetAllocatedUnseenMessages(string userId) => Random.Range(0, 10);

    public int GetAllocatedUnseenChannelMessages(string channelId) => controller.GetAllocatedUnseenChannelMessages(channelId);
    public void CreateChannel(string channelId) => controller.CreateChannel(channelId);

    public void JoinOrCreateChannel(string channelId) => controller.JoinOrCreateChannel(channelId);

    public void LeaveChannel(string channelId) => controller.LeaveChannel(channelId);

    public void GetChannelMessages(string channelId, int limit, long fromTimestamp) =>
        controller.GetChannelMessages(channelId, limit, fromTimestamp);

    public void GetJoinedChannels(int limit, int skip) =>
        controller.GetJoinedChannels(limit, skip);

    public void GetChannels(int limit, int skip, string name) =>
        controller.GetChannels(limit, skip, name);

    public void GetChannels(int limit, int skip) =>
        controller.GetChannels(limit, skip);

    public void MuteChannel(string channelId) => controller.MuteChannel(channelId);

    public Channel GetAllocatedChannel(string channelId) => controller.GetAllocatedChannel(channelId);

    public List<ChatMessage> GetAllocatedEntriesByChannel(string channelId) =>
        controller.GetAllocatedEntriesByChannel(channelId);

    public List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId) => controller.GetPrivateAllocatedEntriesByUser(userId);

    private async UniTask SimulateDelayedResponseFor_GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        await UniTask.Delay(Random.Range(1000, 3000));

        for (int i = limit - 1; i >= 0; i--)
        {
            controller.AddMessageToChatWindow(
                CreateMockedDataFor_AddMessageToChatWindowPayload(
                    userId,
                    $"fake message {i + 1} from user {userId}",
                    DateTimeOffset.FromUnixTimeMilliseconds(fromTimestamp).AddMinutes(-10 * (i + 1)).ToUnixTimeMilliseconds(),
                    Random.Range(0, 2) != 0));
        }
    }

    private string CreateMockedDataFor_AddMessageToChatWindowPayload(string userId, string messageBody, long fromTimestamp, bool isOwnMessage)
    {
        var fakeMessage = new ChatMessage
        {
            body = messageBody,
            sender = isOwnMessage ? UserProfile.GetOwnUserProfile().userId : userId,
            recipient = isOwnMessage ? userId : UserProfile.GetOwnUserProfile().userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong)fromTimestamp
        };

        return JsonUtility.ToJson(fakeMessage);
    }

    private void CreateFakeUsersInCatalog()
    {
        if (UserProfileController.i == null)
            return;

        for (int i = 0; i < MAX_AMOUNT_OF_FAKE_USERS_IN_CATALOG; i++)
        {
            var model = new UserProfileModel
            {
                userId = $"fakeuser{i + 1}",
                name = $"Fake User {i + 1}",
                snapshots = new UserProfileModel.Snapshots { face256 = $"https://picsum.photos/seed/{i + 1}/256" }
            };

            UserProfileController.i.AddUserProfileToCatalog(model);
        }
    }
    
    private async UniTask SimulateDelayedResponseFor_MarkAsSeen(string userId)
    {
        await UniTask.Delay(Random.Range(50, 1000));
        
        var totalPayload = new UpdateTotalUnseenMessagesPayload
        {
            total = Random.Range(0, 100)
        };
        controller.UpdateTotalUnseenMessages(JsonUtility.ToJson(totalPayload));

        var userPayload = new UpdateUserUnseenMessagesPayload
        {
            userId = userId,
            total = 0
        };
        controller.UpdateUserUnseenMessages(JsonUtility.ToJson(userPayload));
    }

    private async UniTask SimulateDelayedResponseFor_TotalUnseenMessagesByUser()
    {
        await UniTask.Delay(Random.Range(50, 1000));
        
        var unseenPrivateMessages = Enumerable.Range(0, MAX_AMOUNT_OF_FAKE_USERS_IN_CATALOG)
            .Select(i => new UpdateTotalUnseenMessagesByUserPayload.UnseenPrivateMessage
            {
                count = Random.Range(0, 10),
                userId = $"fakeuser{i + 1}"
            }).ToArray();

        var payload = new UpdateTotalUnseenMessagesByUserPayload
        {
            unseenPrivateMessages = unseenPrivateMessages
        };

        controller.UpdateTotalUnseenMessagesByUser(JsonUtility.ToJson(payload));
    }
    
    private async UniTask SimulateDelayedResponseFor_ChatInitialization()
    {
        await UniTask.Delay(Random.Range(50, 1000));
        
        var payload = new InitializeChatPayload
        {
            totalUnseenPrivateMessages = Random.Range(0, 100)
        };
        controller.InitializeChat(JsonUtility.ToJson(payload));
    }
}