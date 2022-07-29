using System;
using System.Collections.Generic;
using DCL.Interface;

public class ChatController_Mock : IChatController
{
    private readonly List<ChatMessage> entries = new List<ChatMessage>();

    public int TotalUnseenMessages { get; }
    public event Action<ChatMessage> OnAddMessage;
    public event Action<int> OnTotalUnseenMessagesUpdated;
    public event Action<string, int> OnUserUnseenMessagesUpdated;

    public List<ChatMessage> GetAllocatedEntries() { return entries; }

    public List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId)
    {
        return new List<ChatMessage>();
    }

    public void RaiseAddMessage(ChatMessage chatMessage)
    {
        entries.Add(chatMessage);
        OnAddMessage?.Invoke(chatMessage);
    }

    public void Send(ChatMessage message)
    {
        if (message == null) return;
        entries.Add(message);
        OnAddMessage?.Invoke(message);
    }

    public void MarkMessagesAsSeen(string userId)
    {
    }

    public void GetPrivateMessages(string userId, int limit, string fromMessageId)
    {
    }

    public void GetUnseenMessagesByUser()
    {
    }

    public int GetAllocatedUnseenMessages(string userId) => 0;
}