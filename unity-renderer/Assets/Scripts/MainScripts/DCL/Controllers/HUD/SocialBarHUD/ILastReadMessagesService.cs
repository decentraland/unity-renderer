using System;
using DCL;

public interface ILastReadMessagesService : IService
{
    event Action<string> OnUpdated; 
    void MarkAllRead(string chatId);
    int GetUnreadCount(string chatId);
}