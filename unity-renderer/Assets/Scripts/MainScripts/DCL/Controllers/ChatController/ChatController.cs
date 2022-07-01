using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Chat.WebApi;
using JetBrains.Annotations;
using UnityEngine;

public class ChatController : MonoBehaviour, IChatController
{
    public static ChatController i { get; private set; }
    
    [NonSerialized] public List<ChatMessage> entries = new List<ChatMessage>();

    private readonly Dictionary<string, int> unseenMessagesByUser = new Dictionary<string, int>();

    public void Awake()
    {
        i = this;
    }

    public int TotalUnseenMessages { get; private set; }
    public event Action<ChatMessage> OnAddMessage;
    public event Action<int> OnTotalUnseenMessagesUpdated;
    public event Action<string, int> OnUserUnseenMessagesUpdated;

    // called by kernel
    [UsedImplicitly]
    public void InitializeChat(string json)
    {
        var msg = JsonUtility.FromJson<InitializeChatPayload>(json);

        // TODO: add #nearby unseen messages
        TotalUnseenMessages = msg.totalUnseenPrivateMessages;
    }

    // called by kernel
    [UsedImplicitly]
    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);

        if (message == null)
            return;

        entries.Add(message);
        OnAddMessage?.Invoke(message);
    }
    
    // called by kernel
    [UsedImplicitly]
    public void UpdateTotalUnseenMessages(string json)
    {
        var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesPayload>(json);
        // TODO: add #nearby unseen messages
        TotalUnseenMessages = msg.total;
        OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
    }
    
    // called by kernel
    [UsedImplicitly]
    public void UpdateUserUnseenMessages(string json)
    {
        var msg = JsonUtility.FromJson<UpdateUserUnseenMessagesPayload>(json);
        unseenMessagesByUser[msg.userId] = msg.total;
        OnUserUnseenMessagesUpdated?.Invoke(msg.userId, msg.total);
    }
    
    // called by kernel
    [UsedImplicitly]
    public void UpdateTotalUnseenMessagesByUser(string json)
    {
        var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesByUserPayload>(json);

        foreach (var unseenMessages in msg.unseenPrivateMessages)
        {
            unseenMessagesByUser[unseenMessages.userId] = unseenMessages.count;
            OnUserUnseenMessagesUpdated?.Invoke(unseenMessages.userId, unseenMessages.count);
        }
    }

    public void Send(ChatMessage message) => WebInterface.SendChatMessage(message);

    public void MarkMessagesAsSeen(string userId) => WebInterface.MarkMessagesAsSeen(userId);

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp) { WebInterface.GetPrivateMessages(userId, limit, fromTimestamp); }

    public void GetUnseenMessagesByUser() => WebInterface.GetUnseenMessagesByUser();

    public List<ChatMessage> GetAllocatedEntries() { return new List<ChatMessage>(entries); }

    public List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId)
    {
        return entries
            .Where(x => (x.sender == userId || x.recipient == userId) && x.messageType == ChatMessage.Type.PRIVATE)
            .ToList();
    }

    [ContextMenu("Fake Public Message")]
    public void FakePublicMessage()
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

        var model = new UserProfileModel()
        {
            userId = "test user 1",
            name = "test user 1",
        };

        UserProfileController.i.AddUserProfileToCatalog(model);

        var model2 = new UserProfileModel()
        {
            userId = "test user 2",
            name = "test user 2",
        };

        UserProfileController.i.AddUserProfileToCatalog(model2);

        var msg = new ChatMessage()
        {
            body = "test message",
            sender = model.userId,
            messageType = ChatMessage.Type.PUBLIC,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var msg2 = new ChatMessage()
        {
            body = "test message 2",
            sender = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        AddMessageToChatWindow(JsonUtility.ToJson(msg));
        AddMessageToChatWindow(JsonUtility.ToJson(msg2));
    }

    [ContextMenu("Fake Private Message")]
    public void FakePrivateMessage()
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

        var model = new UserProfileModel()
        {
            userId = "test user 1",
            name = "test user 1",
        };

        UserProfileController.i.AddUserProfileToCatalog(model);

        var model2 = new UserProfileModel()
        {
            userId = "test user 2",
            name = "test user 2",
        };

        UserProfileController.i.AddUserProfileToCatalog(model2);

        var msg = new ChatMessage()
        {
            body = "test message",
            sender = model.userId,
            recipient = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var msg2 = new ChatMessage()
        {
            body = "test message 2",
            recipient = model2.userId,
            sender = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        AddMessageToChatWindow(JsonUtility.ToJson(msg));
        AddMessageToChatWindow(JsonUtility.ToJson(msg2));
    }
}