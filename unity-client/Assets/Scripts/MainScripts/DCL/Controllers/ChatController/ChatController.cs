using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IChatController
{
    double initTime { get; }
    event Action<ChatMessage> OnAddMessage;
    List<ChatMessage> GetEntries();

    void AddMessageToChatWindow(string jsonMessage);
}

public class ChatController : MonoBehaviour, IChatController
{
    public static ChatController i { get; private set; }
    public double initTime { get; private set; }

    public void Awake()
    {
        i = this;
        initTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
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

    public List<ChatMessage> GetEntries()
    {
        return new List<ChatMessage>(entries);
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
            messageType = ChatMessage.Type.PRIVATE
        };

        var msg2 = new ChatMessage()
        {
            body = "test message 2",
            recipient = model2.userId,
            sender = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE
        };

        AddMessageToChatWindow(JsonUtility.ToJson(msg));
        AddMessageToChatWindow(JsonUtility.ToJson(msg2));
    }
}
