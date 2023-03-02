using Cysharp.Threading.Tasks;
using DCL.Chat.Channels;
using DCL.Chat.WebApi;
using DCL.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DCL.Social.Chat
{
    public interface IChatApiBridge
    {
        event Action<InitializeChatPayload> OnInitialized;
        event Action<ChatMessage[]> OnAddMessage;
        event Action<UpdateTotalUnseenMessagesPayload> OnTotalUnseenMessagesChanged;
        event Action<(string userId, int count)[]> OnUserUnseenMessagesChanged;
        event Action<(string channelId, int count)[]> OnChannelUnseenMessagesChanged;
        event Action<UpdateChannelMembersPayload> OnChannelMembersUpdated;
        event Action<ChannelInfoPayloads> OnChannelJoined;
        event Action<JoinChannelErrorPayload> OnChannelJoinFailed;
        event Action<JoinChannelErrorPayload> OnChannelLeaveFailed;
        event Action<ChannelInfoPayloads> OnChannelsUpdated;
        event Action<MuteChannelErrorPayload> OnMuteChannelFailed;
        event Action<ChannelSearchResultsPayload> OnChannelSearchResults;

        // TODO: refactor into promises/tasks instead of events for each request
        void LeaveChannel(string channelId);
        void GetChannelMessages(string channelId, int limit, string fromMessageId);
        UniTask<ChannelInfoPayload> JoinOrCreateChannelAsync(string channelId, CancellationToken cancellationToken = default);
        [Obsolete("Use JoinOrCreateChannelAsync instead")]
        void JoinOrCreateChannel(string channelId);
        void GetJoinedChannels(int limit, int skip);
        void GetChannels(int limit, string paginationToken, string name);
        UniTask<ChannelSearchResultsPayload> GetChannelsAsync(int limit, string name, string paginationToken, CancellationToken cancellationToken = default);
        void MuteChannel(string channelId, bool muted);
        void GetPrivateMessages(string userId, int limit, string fromMessageId);
        void MarkChannelMessagesAsSeen(string channelId);
        void GetUnseenMessagesByUser();
        void GetUnseenMessagesByChannel();
        void CreateChannel(string channelId);
        void GetChannelInfo(string[] channelIds);
        void GetChannelMembers(string channelId, int limit, int skip, string name);
        void SendChatMessage(ChatMessage message);
        void MarkMessagesAsSeen(string userId);
    }
}
