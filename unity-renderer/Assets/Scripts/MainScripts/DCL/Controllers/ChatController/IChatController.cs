using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Interface;

public interface IChatController
{
    event Action<ChatMessage> OnAddMessage;
    
    List<ChatMessage> GetAllocatedEntries();
    void AddMessageToChatWindow(string jsonMessage);
    void Send(ChatMessage message);
    void MarkMessagesAsSeen(string userId);
    UniTask<List<ChatMessage>> GetPrivateMessages(string userId, int limit, long fromTimestamp);
}