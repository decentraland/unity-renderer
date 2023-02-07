using Cysharp.Threading.Tasks;
using DCL.Chat.Channels;
using DCL.Chat.WebApi;
using DCL.Interface;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class DummyChatApiBridge : IChatApiBridge
    {
        public event Action<InitializeChatPayload> OnInitialized;
        public event Action<ChatMessage[]> OnAddMessage;
        public event Action<UpdateTotalUnseenMessagesPayload> OnTotalUnseenMessagesChanged;
        public event Action<(string userId, int count)[]> OnUserUnseenMessagesChanged;
        public event Action<(string channelId, int count)[]> OnChannelUnseenMessagesChanged;
        public event Action<UpdateChannelMembersPayload> OnChannelMembersUpdated;
        public event Action<ChannelInfoPayloads> OnChannelJoined;
        public event Action<JoinChannelErrorPayload> OnChannelJoinFailed;
        public event Action<JoinChannelErrorPayload> OnChannelLeaveFailed;
        public event Action<ChannelInfoPayloads> OnChannelsUpdated;
        public event Action<MuteChannelErrorPayload> OnMuteChannelFailed;
        public event Action<ChannelSearchResultsPayload> OnChannelSearchResults;

        public void LeaveChannel(string channelId)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetChannelMessages(string channelId, int limit, string fromMessageId)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public UniTask<ChannelInfoPayload> JoinOrCreateChannelAsync(string channelId, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
            return UniTask.FromResult(new ChannelInfoPayload
            {
                channelId = channelId
            });
        }

        public void JoinOrCreateChannel(string channelId)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetJoinedChannels(int limit, int skip)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetChannels(int limit, string paginationToken, string name)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public UniTask<ChannelSearchResultsPayload> GetChannelsAsync(int limit, string name, string paginationToken, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
            return UniTask.FromResult(new ChannelSearchResultsPayload
            {
                channels = Array.Empty<ChannelInfoPayload>(),
            });
        }

        public void MuteChannel(string channelId, bool muted)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetPrivateMessages(string userId, int limit, string fromMessageId)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void MarkChannelMessagesAsSeen(string channelId)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetUnseenMessagesByUser()
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetUnseenMessagesByChannel()
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void CreateChannel(string channelId)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetChannelInfo(string[] channelIds)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void GetChannelMembers(string channelId, int limit, int skip, string name)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void SendChatMessage(ChatMessage message)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }

        public void MarkMessagesAsSeen(string userId)
        {
            Debug.LogWarning("Usage of DummyChatApiBridge");
        }
    }
}
