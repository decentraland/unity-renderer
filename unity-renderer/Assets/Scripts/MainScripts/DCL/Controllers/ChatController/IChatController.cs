using DCL.Interface;
using System;
using System.Collections.Generic;
using DCL.Chat.Channels;

public interface IChatController
{
    event Action<ChatMessage> OnAddMessage;
    event Action<Channel> OnChannelUpdated;
    event Action<Channel> OnChannelJoined;
    event Action<string, string> OnJoinChannelError;
    event Action<string> OnChannelLeft;
    event Action<string, string> OnChannelLeaveError;
    event Action<string, string> OnMuteChannelError;
    event Action<int> OnTotalUnseenMessagesUpdated;
    event Action<string, int> OnUserUnseenMessagesUpdated;
    event Action<string, int> OnChannelUnseenMessagesUpdated;

    int TotalUnseenMessages { get; }

    List<ChatMessage> GetAllocatedEntries();
    List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId);
    void Send(ChatMessage message);
    void MarkMessagesAsSeen(string userId);
    void GetPrivateMessages(string userId, int limit, string fromMessageId);
    void MarkChannelMessagesAsSeen(string channelId);
    void JoinOrCreateChannel(string channelId);
    void LeaveChannel(string channelId);
    void GetChannelMessages(string channelId, int limit, long fromTimestamp);
    void GetJoinedChannels(int limit, int skip);
    void GetChannels(int limit, int skip, string name);
    void GetChannels(int limit, int skip);
    void MuteChannel(string channelId);
    Channel GetAllocatedChannel(string channelId);
    List<ChatMessage> GetAllocatedEntriesByChannel(string channelId);
    void GetUnseenMessagesByUser();
    void GetUnseenMessagesByChannel();
    int GetAllocatedUnseenMessages(string userId);
    int GetAllocatedUnseenChannelMessages(string channelId);
    void CreateChannel(string channelId);
}