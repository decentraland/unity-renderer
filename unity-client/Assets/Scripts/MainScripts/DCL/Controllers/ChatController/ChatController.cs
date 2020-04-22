using System;
using System.Collections.Generic;
using UnityEngine;

public interface IChatController
{
    event Action<ChatController.ChatMessage> OnAddMessage;
    List<ChatController.ChatMessage> GetEntries();
}

public class ChatController : MonoBehaviour, IChatController
{
    public static ChatController i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    public enum ChatMessageType
    {
        NONE,
        PUBLIC,
        PRIVATE,
        SYSTEM
    }

    [System.Serializable]
    public class ChatMessage
    {
        public ChatMessageType messageType;
        public string sender;
        public string recipient;
        public ulong timestamp;
        public string body;
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
}
