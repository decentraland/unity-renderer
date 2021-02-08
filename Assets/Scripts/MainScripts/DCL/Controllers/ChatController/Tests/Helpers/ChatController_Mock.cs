using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChatController_Mock : IChatController
{
    public event Action<ChatMessage> OnAddMessage;
    List<ChatMessage> entries = new List<ChatMessage>();

    public double initTime => 0;

    public List<ChatMessage> GetEntries()
    {
        return entries;
    }

    public void RaiseAddMessage(ChatMessage chatMessage)
    {
        entries.Add(chatMessage);
        OnAddMessage?.Invoke(chatMessage);
    }

    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);

        if (message == null)
            return;

        entries.Add(message);
        OnAddMessage?.Invoke(message);
    }
}
