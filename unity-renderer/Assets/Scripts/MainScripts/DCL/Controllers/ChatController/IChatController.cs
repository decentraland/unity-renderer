using DCL.Interface;
using System;
using System.Collections.Generic;

public interface IChatController
{
    event Action<ChatMessage> OnAddMessage;
    
    List<ChatMessage> GetAllocatedEntries();
    List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId);
    void Send(ChatMessage message);
    void MarkMessagesAsSeen(string userId);
    void GetPrivateMessages(string userId, int limit, long fromTimestamp);
}