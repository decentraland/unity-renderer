using DCL;using DCL.Chat.Channels;
using DCL.Interface;
using System;
using System.Threading;

public interface IChatController : IService
{
    event Action OnInitialized;
    event Action<ChatMessage[]> OnAddMessage;
    event Action<Channel> OnChannelUpdated;
    event Action<Channel> OnChannelJoined;
    event Action<Channel> OnAutoChannelJoined;
    event Action<string, ChannelErrorCode> OnJoinChannelError;
    event Action<string> OnChannelLeft;
    event Action<string, ChannelErrorCode> OnChannelLeaveError;
    event Action<string, ChannelErrorCode> OnMuteChannelError;
    event Action<int> OnTotalUnseenMessagesUpdated;
    event Action<string, int> OnUserUnseenMessagesUpdated;
    event Action<string, int> OnChannelUnseenMessagesUpdated;
    event Action<string, ChannelMember[]> OnUpdateChannelMembers;
    event Action<string, Channel[]> OnChannelSearchResult;
    event Action<string> OnAskForJoinChannel;

    int TotalUnseenMessages { get; }
    bool IsInitialized { get; }

    void Send(ChatMessage message);
    void MarkMessagesAsSeen(string userId);
    void GetPrivateMessages(string userId, int limit, string fromMessageId);
    void MarkChannelMessagesAsSeen(string channelId);

    Cysharp.Threading.Tasks.UniTask<Channel> JoinOrCreateChannelAsync(string channelId, CancellationToken cancellationToken = default);
    [Obsolete("Use JoinOrCreateChannelAsync instead")]
    void JoinOrCreateChannel(string channelId);
    void LeaveChannel(string channelId);
    void GetChannelMessages(string channelId, int limit, string fromMessageId);
    void GetJoinedChannels(int limit, int skip);

    Cysharp.Threading.Tasks.UniTask<(string, Channel[])> GetChannelsByNameAsync(int limit, string name, string paginationToken = null, CancellationToken cancellationToken = default);
    [Obsolete("Use GetChannelsByNameAsync instead")]
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
