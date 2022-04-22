using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using DCL.Interface;
using Newtonsoft.Json;

public class LastReadMessagesService : ILastReadMessagesService
{
    private const string PLAYER_PREFS_LAST_READ_CHAT_MESSAGES = "LastReadChatMessages";

    private readonly ReadMessagesDictionary memoryRepository;
    private readonly IChatController chatController;
    private readonly IPlayerPrefs persistentRepository;
    
    public event Action<string> OnUpdated;
    
    public LastReadMessagesService(ReadMessagesDictionary memoryRepository,
        IChatController chatController,
        IPlayerPrefs persistentRepository)
    {
        this.memoryRepository = memoryRepository;
        this.chatController = chatController;
        this.persistentRepository = persistentRepository;
    }

    public void Initialize() => Load();

    public void MarkAllRead(string chatId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        memoryRepository.Remove(chatId);
        memoryRepository.Add(chatId, timestamp);
        Persist();
        OnUpdated?.Invoke(chatId);
    }

    public int GetUnreadCount(string chatId)
    {
        var timestamp = Get(chatId);

        return chatController.GetEntries()
            .Count(
                msg => msg.messageType == ChatMessage.Type.PRIVATE &&
                       msg.sender == chatId &&
                       msg.timestamp >= timestamp);
    }

    public void Dispose()
    {
    }

    private void Load()
    {
        var lastReadChatMessagesList =
            JsonConvert.DeserializeObject<List<KeyValuePair<string, long>>>(
                persistentRepository.GetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES));
        if (lastReadChatMessagesList == null) return;
        foreach (var item in lastReadChatMessagesList)
            memoryRepository.Add(item.Key, item.Value);
    }

    private ulong Get(string chatId) =>
        (ulong) (memoryRepository.ContainsKey(chatId) ? memoryRepository.Get(chatId) : 0);

    private void Persist()
    {
        var lastReadChatMessagesList = new List<KeyValuePair<string, long>>();
        using (var iterator = memoryRepository.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                lastReadChatMessagesList.Add(new KeyValuePair<string, long>(iterator.Current.Key,
                    iterator.Current.Value));
            }
        }

        persistentRepository.Set(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES,
            JsonConvert.SerializeObject(lastReadChatMessagesList));
        persistentRepository.Save();
    }
}