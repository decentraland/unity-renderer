using DCL.Interface;
using System;
using System.Collections.Generic;
using DCL.Chat.Channels;
using UnityEngine;

public class ChatController_Mock : IChatController
{
    private readonly List<ChatMessage> entries = new List<ChatMessage>();
    
    public event Action<ChatMessage> OnAddMessage;
    public event Action OnInitialized;
    public event Action<Channel> OnChannelUpdated;
    public event Action<Channel> OnChannelJoined;
    public event Action<string, string> OnJoinChannelError;
    public event Action<string> OnChannelLeft;
    public event Action<string, string> OnChannelLeaveError;
    public event Action<string, string> OnMuteChannelError;

    public double initTime => 0;

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

    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);

        if (message == null)
            return;

        entries.Add(message);
        OnAddMessage?.Invoke(message);
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

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
    }

    public void JoinOrCreateChannel(string channelId)
    {
    }

    public void LeaveChannel(string channelId)
    {
    }

    public void GetChannelMessages(string channelId, int limit, long fromTimestamp)
    {
    }

    public void GetJoinedChannels(int limit, int skip)
    {
    }

    public void GetChannels(int limit, int skip, string name)
    {
    }

    public void MuteChannel(string channelId)
    {
    }
}