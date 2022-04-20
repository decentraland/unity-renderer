using DCL;

public interface ILastReadMessagesService : IService
{   
    void MarkAllRead(string chatId);
    int GetUnreadCount(string chatId);
}