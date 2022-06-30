using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Chat.Channels;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

public class ChatController : MonoBehaviour, IChatController
{
    public static ChatController i { get; private set; }

    private readonly Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
    private readonly List<ChatMessage> messages = new List<ChatMessage>();
    private readonly Random randomizer = new Random();

    public event Action OnInitialized;
    public event Action<Channel> OnChannelUpdated;
    public event Action<Channel> OnChannelJoined;
    public event Action<string, string> OnJoinChannelError;
    public event Action<string> OnChannelLeft;
    public event Action<string, string> OnChannelLeaveError;
    public event Action<string, string> OnMuteChannelError;
    public event Action<ChatMessage> OnAddMessage;
    
    public int TotalJoinedChannelCount => throw new NotImplementedException();

    public void Awake()
    {
        i = this;
    }

    // called by kernel
    [UsedImplicitly]
    public void InitializeChannels(string payload)
    {
        var msg = JsonUtility.FromJson<InitializeChannelsPayload>(payload);
        // TODO: add unseen notifications
        OnInitialized?.Invoke();
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

    public void LeaveChannel(string channelId)
    {
        throw new NotImplementedException();
    }

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

    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);

        if (message == null)
            return;

        messages.Add(message);
        OnAddMessage?.Invoke(message);
    }

    public void Send(ChatMessage message) => WebInterface.SendChatMessage(message);

    public void MarkMessagesAsSeen(string userId)
    {
        WebInterface.MarkMessagesAsSeen(userId);
    }

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        WebInterface.GetPrivateMessages(userId, limit, fromTimestamp);
    }

    public List<ChatMessage> GetAllocatedEntries()
    {
        return new List<ChatMessage>(messages);
    }

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