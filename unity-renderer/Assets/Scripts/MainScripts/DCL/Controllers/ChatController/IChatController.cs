using DCL.Interface;
using System;
using System.Collections.Generic;

public interface IChatController
{
    event Action<ChatMessage> OnAddMessage;
    
    List<ChatMessage> GetAllocatedEntries();
    void Send(ChatMessage message);
    void MarkMessagesAsSeen(string userId);
    void GetPrivateMessages(string userId, int limit, long fromTimestamp);
}