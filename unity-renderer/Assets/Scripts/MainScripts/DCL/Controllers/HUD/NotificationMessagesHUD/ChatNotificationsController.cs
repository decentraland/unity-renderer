using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IChatNotificationsController
{
    event Action<ChatMessage> OnAddMessage;

    void AddMessageToChatWindow(string jsonMessage);
    List<ChatMessage> GetEntries();
}

public class ChatNotificationsController : MonoBehaviour, IChatNotificationsController
{
    [NonSerialized] public List<ChatMessage> entries = new List<ChatMessage>();
    public event Action<ChatMessage> OnAddMessage;

    public static ChatNotificationsController i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);
        if (message == null)
            return;

        entries.Add(message);
        OnAddMessage?.Invoke(message);
    }

    public List<ChatMessage> GetEntries() { return new List<ChatMessage>(entries); }

}