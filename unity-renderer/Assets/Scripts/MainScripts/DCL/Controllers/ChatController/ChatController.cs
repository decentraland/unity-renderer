using DCL.Interface;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ChatController : MonoBehaviour, IChatController
{
    public static ChatController i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    [NonSerialized] public List<ChatMessage> entries = new List<ChatMessage>();

    public event Action<ChatMessage> OnAddMessage;

    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);

        if (message == null)
            return;

        entries.Add(message);
        OnAddMessage?.Invoke(message);
    }

    public void Send(ChatMessage message) => WebInterface.SendChatMessage(message);
    
    public void MarkMessagesAsSeen(string userId)
    {
        throw new NotImplementedException();
    }

    public UniTask<List<ChatMessage>> GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        throw new NotImplementedException();
    }

    public List<ChatMessage> GetAllocatedEntries() { return new List<ChatMessage>(entries); }
    
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