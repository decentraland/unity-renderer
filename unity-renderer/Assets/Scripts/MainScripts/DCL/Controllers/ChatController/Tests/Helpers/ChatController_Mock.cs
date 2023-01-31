using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using DCL.Chat.Channels;
using DCL.Interface;
using System.Threading;
using Channel = DCL.Chat.Channels.Channel;

public class ChatController_Mock : IChatController
{
    private readonly List<ChatMessage> entries = new ();

    public event Action OnInitialized;
    public event Action<ChatMessage[]> OnAddMessage;
    public event Action<Channel> OnChannelUpdated;
    public event Action<Channel> OnChannelJoined;
    public event Action<Channel> OnAutoChannelJoined;
    public event Action<string, ChannelErrorCode> OnJoinChannelError;
    public event Action<string> OnChannelLeft;
    public event Action<string, ChannelErrorCode> OnChannelLeaveError;
    public event Action<string, ChannelErrorCode> OnMuteChannelError;
    public event Action<int> OnTotalUnseenMessagesUpdated;
    public event Action<string, int> OnUserUnseenMessagesUpdated;
    public event Action<string, int> OnChannelUnseenMessagesUpdated;
    public event Action<string, ChannelMember[]> OnUpdateChannelMembers;
    public event Action<string, Channel[]> OnChannelSearchResult;
    public event Action<string> OnAskForJoinChannel;

    public int TotalUnseenMessages { get; }
    public bool IsInitialized { get; }

    public void Send(ChatMessage message)
    {
        if (message == null) return;
        entries.Add(message);
        OnAddMessage?.Invoke(new[] {message});
    }

    public void MarkMessagesAsSeen(string userId)
    {
    }

    public void GetPrivateMessages(string userId, int limit, string fromMessageId)
    {
    }

    public void MarkChannelMessagesAsSeen(string channelId)
    {
    }

    public UniTask<Channel> JoinOrCreateChannelAsync(string channelId, CancellationToken cancellationToken = default) =>
        UniTask.FromResult(new Channel(channelId, "", 0, 0, false, false, ""));

    public void JoinOrCreateChannel(string channelId)
    {
    }

    public void LeaveChannel(string channelId)
    {
    }

    public void GetChannelMessages(string channelId, int limit, string fromMessageId)
    {
    }

    public void GetJoinedChannels(int limit, int skip)
    {
    }

    public UniTask<(string, Channel[])> GetChannelsByNameAsync(int limit, string name, string paginationToken = null, CancellationToken cancellationToken = default) =>
        UniTask.FromResult(("", Array.Empty<Channel>()));

    public void GetChannelsByName(int limit, string name, string paginationToken = null)
    {
    }

    public void GetChannels(int limit, string paginationToken)
    {
    }

    public void MuteChannel(string channelId)
    {
    }

    public void UnmuteChannel(string channelId)
    {
    }

    public Channel GetAllocatedChannel(string channelId) => null;

    public void GetUnseenMessagesByUser()
    {
    }

    public void GetUnseenMessagesByChannel()
    {
    }

    public int GetAllocatedUnseenMessages(string userId) => 0;

    public int GetAllocatedUnseenChannelMessages(string channelId) => 0;

    public void CreateChannel(string channelId)
    {
    }

    public void GetChannelInfo(string[] channelIds)
    {
    }

    public void GetChannelMembers(string channelId, int limit, int skip, string name)
    {
    }

    public void GetChannelMembers(string channelId, int limit, int skip)
    {
    }

    public Channel GetAllocatedChannelByName(string channelName) => null;
}
