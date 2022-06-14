using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using DCL.Interface;
using Newtonsoft.Json;
using UnityEngine;

public class LastReadMessagesService : ILastReadMessagesService
{
    private const string PLAYER_PREFS_LAST_READ_CHAT_MESSAGES = "LastReadChatMessages";

    private readonly ReadMessagesDictionary memoryRepository;
    private readonly IChatController chatController;
    private readonly IPlayerPrefs persistentRepository;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly Dictionary<string, int> channelUnreadCount = new Dictionary<string, int>();

    public event Action<string> OnUpdated;

    public LastReadMessagesService(ReadMessagesDictionary memoryRepository,
        IChatController chatController,
        IPlayerPrefs persistentRepository,
        IUserProfileBridge userProfileBridge)
    {
        this.memoryRepository = memoryRepository;
        this.chatController = chatController;
        this.persistentRepository = persistentRepository;
        this.userProfileBridge = userProfileBridge;
    }

    public void Initialize()
    {
        if (chatController != null)
            chatController.OnAddMessage += HandleMessageReceived;
        LoadLastReadTimestamps();
        LoadChannelsUnreadCount();
    }

    public void MarkAllRead(string chatId)
    {
        if (string.IsNullOrEmpty(chatId))
        {
            Debug.LogWarning("Trying to clear last read messages for an empty chatId");
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        memoryRepository.Remove(chatId);
        memoryRepository.Add(chatId, timestamp);
        channelUnreadCount.Remove(chatId);
        Persist();
        OnUpdated?.Invoke(chatId);
    }

    public int GetUnreadCount(string chatId)
    {
        if (string.IsNullOrEmpty(chatId))
        {
            Debug.LogWarning("Trying to get unread messages count for an empty chatId");
            return 0;
        }

        return channelUnreadCount.ContainsKey(chatId) ? channelUnreadCount[chatId] : 0;
    }

    public int GetAllUnreadCount() => channelUnreadCount.Sum(pair => pair.Value);

    public void Dispose()
    {
        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;
    }

    private void LoadLastReadTimestamps()
    {
        var lastReadChatMessagesList =
            JsonConvert.DeserializeObject<List<KeyValuePair<string, long>>>(
                persistentRepository.GetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES));
        if (lastReadChatMessagesList == null) return;
        foreach (var item in lastReadChatMessagesList)
            memoryRepository.Add(item.Key, item.Value);
    }

    private void LoadChannelsUnreadCount()
    {
        if (chatController == null) return;
        var ownUserId = userProfileBridge.GetOwn().userId;
        using var iterator = memoryRepository.GetEnumerator();
        while (iterator.MoveNext())
        {
            var channelId = iterator.Current.Key;
            var timestamp = GetLastReadTimestamp(channelId);
            channelUnreadCount[channelId] = chatController.GetAllocatedEntries()
                .Count(message =>
                {
                    var messageChannelId = GetChannelId(message);
                    if (string.IsNullOrEmpty(messageChannelId)) return false;
                    if (messageChannelId == ownUserId) return false;
                    return messageChannelId == channelId
                           && message.timestamp >= timestamp;
                });;
        }
    }

    private void HandleMessageReceived(ChatMessage message)
    {
        var channelId = GetChannelId(message);
        if (string.IsNullOrEmpty(channelId)) return;
        if (channelId == userProfileBridge.GetOwn().userId) return;
        
        var timestamp = GetLastReadTimestamp(channelId);
        if (message.timestamp >= timestamp)
        {
            if (!channelUnreadCount.ContainsKey(channelId))
                channelUnreadCount[channelId] = 0;
            channelUnreadCount[channelId]++;
        }

        OnUpdated?.Invoke(channelId);
    }

    private string GetChannelId(ChatMessage message)
    {
        return message.messageType switch
        {
            ChatMessage.Type.PRIVATE => message.sender,
            // TODO: solve public channelId from chatMessage information when is done from catalyst side
            ChatMessage.Type.PUBLIC => "general",
            _ => null
        };
    }

    private ulong GetLastReadTimestamp(string chatId)
    {
        var oldNotificationsFilteringTimestamp = DateTime.UtcNow
            .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .Subtract(TimeSpan.FromDays(1))
            .TotalMilliseconds;
        
        return (ulong) (memoryRepository.ContainsKey(chatId)
            ? memoryRepository.Get(chatId)
            : oldNotificationsFilteringTimestamp);
    }

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