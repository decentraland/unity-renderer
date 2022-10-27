using DCL.Interface;
using System;
using System.Collections.Generic;
using DCL.Chat.Channels;

public interface IChatController
{
    event Action OnInitialized;
    event Action<ChatMessage> OnAddMessage;
    event Action<Channel> OnChannelUpdated;
    event Action<Channel> OnChannelJoined;
    event Action<string, ChannelErrorCode> OnJoinChannelError;
    event Action<string> OnChannelLeft;
    event Action<string, ChannelErrorCode> OnChannelLeaveError;
    event Action<string, ChannelErrorCode> OnMuteChannelError;
    event Action<int> OnTotalUnseenMessagesUpdated;
    event Action<string, int> OnUserUnseenMessagesUpdated;
    event Action<string, int> OnChannelUnseenMessagesUpdated;
    event Action<string, ChannelMember[]> OnUpdateChannelMembers;
    event Action<string, Channel[]> OnChannelSearchResult;

    int TotalUnseenMessages { get; }

    List<ChatMessage> GetAllocatedEntries();
    List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId);
    void Send(ChatMessage message);
    void MarkMessagesAsSeen(string userId);
    void GetPrivateMessages(string userId, int limit, string fromMessageId);
    void MarkChannelMessagesAsSeen(string channelId);
    void JoinOrCreateChannel(string channelId);
    void LeaveChannel(string channelId);
    void GetChannelMessages(string channelId, int limit, string fromMessageId);
    void GetJoinedChannels(int limit, int skip);
    void GetChannelsByName(int limit, string name, string paginationToken = null);
    void GetChannels(int limit, string paginationToken = null);
    void MuteChannel(string channelId);
    void UnmuteChannel(string channelId);
    Channel GetAllocatedChannel(string channelId);
    Channel GetAllocatedChannelByName(string channelName);
    void GetUnseenMessagesByUser();
    void GetUnseenMessagesByChannel();
    int GetAllocatedUnseenMessages(string userId);
    int GetAllocatedUnseenChannelMessages(string channelId);
    void CreateChannel(string channelId);
    void GetChannelInfo(string[] channelIds);
    void GetChannelMembers(string channelId, int limit, int skip, string name);
    void GetChannelMembers(string channelId, int limit, int skip);
}