using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Chat.Channels;
using DCL.Chat.WebApi;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

public class ChatController : MonoBehaviour, IChatController
{
    public static ChatController i { get; private set; }

    private readonly Dictionary<string, int> unseenMessagesByUser = new Dictionary<string, int>();
    private readonly Dictionary<string, int> unseenMessagesByChannel = new Dictionary<string, int>();

    private readonly Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
    private readonly List<ChatMessage> messages = new List<ChatMessage>();
    private readonly Random randomizer = new Random();
    private bool chatAlreadyInitialized = false;

    public event Action<Channel> OnChannelUpdated;
    public event Action<Channel> OnChannelJoined;
    public event Action<string, string> OnJoinChannelError;
    public event Action<string> OnChannelLeft;
    public event Action<string, string> OnChannelLeaveError;
    public event Action<string, string> OnMuteChannelError;
    public event Action<ChatMessage> OnAddMessage;
    public event Action<int> OnTotalUnseenMessagesUpdated;
    public event Action<string, int> OnUserUnseenMessagesUpdated;
    public event Action<string, int> OnChannelUnseenMessagesUpdated;

    public int TotalJoinedChannelCount => throw new NotImplementedException();
    public int TotalUnseenMessages { get; private set; }

    public void Awake()
    {
        i = this;
    }

    [UsedImplicitly]
    public void UpdateChannelInfo(string payload)
    {
        var msg = JsonUtility.FromJson<ChannelInfoPayload>(payload);
        var channelId = msg.channelId;
        var channel = new Channel(channelId, msg.unseenMessages, msg.memberCount, msg.joined, msg.muted, msg.description,
            (long) (randomizer.NextDouble() * DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        var justLeft = false;

        if (channels.ContainsKey(channelId))
        {
            justLeft = channels[channelId].Joined && !channel.Joined;
            channels[channelId].CopyFrom(channel);
        }
        else
            channels[channelId] = channel;

        if (justLeft)
            OnChannelLeft?.Invoke(channelId);

        OnChannelUpdated?.Invoke(channel);
    }

    // called by kernel
    [UsedImplicitly]
    public void JoinChannelConfirmation(string payload)
    {
        var msg = JsonUtility.FromJson<ChannelInfoPayload>(payload);
        var channel = new Channel(msg.channelId, msg.unseenMessages, msg.memberCount, msg.joined, msg.muted, msg.description,
            (long) (randomizer.NextDouble() * DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        var channelId = channel.ChannelId;
        
        if (channels.ContainsKey(channelId))
            channels[channelId].CopyFrom(channel);
        else
            channels[channelId] = channel;
        
        OnChannelJoined?.Invoke(channel);
        OnChannelUpdated?.Invoke(channel);
    }

    // called by kernel
    [UsedImplicitly]
    public void JoinChannelError(string payload)
    {
        var msg = JsonUtility.FromJson<JoinChannelErrorPayload>(payload);
        OnJoinChannelError?.Invoke(msg.channelId, msg.message);
    }

    // called by kernel
    [UsedImplicitly]
    public void LeaveChannelError(string payload)
    {
        var msg = JsonUtility.FromJson<JoinChannelErrorPayload>(payload);
        OnChannelLeaveError?.Invoke(msg.channelId, msg.message);
    }

    // called by kernel
    [UsedImplicitly]
    public void MuteChannelError(string payload)
    {
        var msg = JsonUtility.FromJson<MuteChannelErrorPayload>(payload);
        OnMuteChannelError?.Invoke(msg.channelId, msg.message);
    }

    public void JoinOrCreateChannel(string channelId)
    {
        throw new NotImplementedException();
    }

    public void LeaveChannel(string channelId) => WebInterface.LeaveChannel(channelId);

    public void GetChannelMessages(string channelId, int limit, long fromTimestamp)
    {
        throw new NotImplementedException();
    }

    public void GetJoinedChannels(int limit, int skip)
    {
        throw new NotImplementedException();
    }

    public void GetChannels(int limit, int skip, string name)
    {
        throw new NotImplementedException();
    }

    public void GetChannels(int limit, int skip)
    {
        throw new NotImplementedException();
    }

    public void MuteChannel(string channelId)
    {
        throw new NotImplementedException();
    }

    public Channel GetAllocatedChannel(string channelId) =>
        channels.ContainsKey(channelId) ? channels[channelId] : null;

    public List<ChatMessage> GetAllocatedEntriesByChannel(string channelId)
    {
        return messages.Where(message => message.recipient == channelId).ToList();
    }

    // called by kernel
    [UsedImplicitly]
    public void InitializeChat(string json)
    {
        if (chatAlreadyInitialized)
            return;

        var msg = JsonUtility.FromJson<InitializeChatPayload>(json);
        TotalUnseenMessages += msg.totalUnseenMessages;
        OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
        chatAlreadyInitialized = true;
    }

    // called by kernel
    [UsedImplicitly]
    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);

        if (message == null)
            return;

        messages.Add(message);
        OnAddMessage?.Invoke(message);
    }

    // called by kernel
    [UsedImplicitly]
    public void UpdateTotalUnseenMessages(string json)
    {
        var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesPayload>(json);
        TotalUnseenMessages = msg.total;
        OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
    }

    // called by kernel
    [UsedImplicitly]
    public void UpdateUserUnseenMessages(string json)
    {
        var msg = JsonUtility.FromJson<UpdateUserUnseenMessagesPayload>(json);
        unseenMessagesByUser[msg.userId] = msg.total;
        OnUserUnseenMessagesUpdated?.Invoke(msg.userId, msg.total);
    }

    // called by kernel
    [UsedImplicitly]
    public void UpdateTotalUnseenMessagesByUser(string json)
    {
        var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesByUserPayload>(json);

        foreach (var unseenMessages in msg.unseenPrivateMessages)
        {
            unseenMessagesByUser[unseenMessages.Key] = unseenMessages.Value;
            OnUserUnseenMessagesUpdated?.Invoke(unseenMessages.Key, unseenMessages.Value);
        }
    }

    // called by kernel
    [UsedImplicitly]
    public void UpdateTotalUnseenMessagesByChannel(string json)
    {
        var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesByChannelPayload>(json);

        foreach (var unseenMessages in msg.unseenChannelMessages)
        {
            unseenMessagesByChannel[unseenMessages.channelId] = unseenMessages.count;
            OnChannelUnseenMessagesUpdated?.Invoke(unseenMessages.channelId, unseenMessages.count);
        }
    }

    public void Send(ChatMessage message) => WebInterface.SendChatMessage(message);

    public void MarkMessagesAsSeen(string userId) => WebInterface.MarkMessagesAsSeen(userId);

    public void MarkChannelMessagesAsSeen(string channelId) => WebInterface.MarkChannelMessagesAsSeen(channelId);

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp) => WebInterface.GetPrivateMessages(userId, limit, fromTimestamp);

    public List<ChatMessage> GetAllocatedEntries() => new List<ChatMessage>(messages);
    
    public void GetUnseenMessagesByUser() => WebInterface.GetUnseenMessagesByUser();

    public void GetUnseenMessagesByChannel() => WebInterface.GetUnseenMessagesByChannel();

    public int GetAllocatedUnseenMessages(string userId) =>
        unseenMessagesByUser.ContainsKey(userId) ? unseenMessagesByUser[userId] : 0;

    public int GetAllocatedUnseenChannelMessages(string channelId) =>
        unseenMessagesByChannel.ContainsKey(channelId) ? unseenMessagesByChannel[channelId] : 0;

    public List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId)
    {
        return messages
            .Where(x => (x.sender == userId || x.recipient == userId) && x.messageType == ChatMessage.Type.PRIVATE)
            .ToList();
    }

    [ContextMenu("Fake Public Message")]
    public void FakePublicMessage()
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

        var model = new UserProfileModel()
        {
            userId = "test user 1",
            name = "test user 1",
        };

        UserProfileController.i.AddUserProfileToCatalog(model);

        var model2 = new UserProfileModel()
        {
            userId = "test user 2",
            name = "test user 2",
        };

        UserProfileController.i.AddUserProfileToCatalog(model2);

        var msg = new ChatMessage()
        {
            body = "test message",
            sender = model.userId,
            messageType = ChatMessage.Type.PUBLIC,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var msg2 = new ChatMessage()
        {
            body = "test message 2",
            sender = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        AddMessageToChatWindow(JsonUtility.ToJson(msg));
        AddMessageToChatWindow(JsonUtility.ToJson(msg2));
    }

    [ContextMenu("Fake Private Message")]
    public void FakePrivateMessage()
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

        var model = new UserProfileModel()
        {
            userId = "test user 1",
            name = "test user 1",
        };

        UserProfileController.i.AddUserProfileToCatalog(model);

        var model2 = new UserProfileModel()
        {
            userId = "test user 2",
            name = "test user 2",
        };

        UserProfileController.i.AddUserProfileToCatalog(model2);

        var msg = new ChatMessage()
        {
            body = "test message",
            sender = model.userId,
            recipient = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var msg2 = new ChatMessage()
        {
            body = "test message 2",
            recipient = model2.userId,
            sender = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        AddMessageToChatWindow(JsonUtility.ToJson(msg));
        AddMessageToChatWindow(JsonUtility.ToJson(msg2));
    }
}