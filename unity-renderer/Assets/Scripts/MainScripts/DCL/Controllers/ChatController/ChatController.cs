using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Chat.Channels;
using DCL.Chat.WebApi;
using DCL.Interface;
using System.Threading;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Social.Chat
{
    public partial class ChatController : IChatController
    {
        private const string NEARBY_CHANNEL_DESCRIPTION =
            "Talk to the people around you. If you move far away from someone you will lose contact. All whispers will be displayed.";

        private const string NEARBY_CHANNEL_ID = "nearby";

        private readonly Dictionary<string, int> unseenMessagesByUser = new ();
        private readonly Dictionary<string, int> unseenMessagesByChannel = new ();
        private readonly Dictionary<string, Channel> channels = new ();
        private HashSet<string> autoJoinChannelList => dataStore.HUDs.autoJoinChannelList.Get();
        private bool chatAlreadyInitialized;
        private int totalUnseenMessages;
        private readonly DataStore dataStore;
        private readonly IChatApiBridge apiBridge;
        private readonly CancellationTokenSource operationsCancellationToken = new ();

        public event Action<Channel> OnChannelUpdated;
        public event Action<Channel> OnChannelJoined;
        public event Action<Channel> OnAutoChannelJoined;
        public event Action<string, ChannelErrorCode> OnJoinChannelError;
        public event Action<string> OnChannelLeft;
        public event Action<string, ChannelErrorCode> OnChannelLeaveError;
        public event Action<string, ChannelErrorCode> OnMuteChannelError;
        public event Action OnInitialized;
        public event Action<ChatMessage[]> OnAddMessage;
        public event Action<int> OnTotalUnseenMessagesUpdated;
        public event Action<string, int> OnUserUnseenMessagesUpdated;
        public event Action<string, ChannelMember[]> OnUpdateChannelMembers;
        public event Action<string, Channel[]> OnChannelSearchResult;
        public event Action<string, int> OnChannelUnseenMessagesUpdated;
        public event Action<string> OnAskForJoinChannel;

        // since kernel does not calculate the #nearby channel unseen messages, it is handled on renderer side
        public int TotalUnseenMessages => totalUnseenMessages
                                          + (unseenMessagesByChannel.ContainsKey(NEARBY_CHANNEL_ID)
                                              ? unseenMessagesByChannel[NEARBY_CHANNEL_ID]
                                              : 0);

        public bool IsInitialized => chatAlreadyInitialized;

        public ChatController(IChatApiBridge apiBridge,
            DataStore dataStore)
        {
            this.dataStore = dataStore;
            this.apiBridge = apiBridge;

            channels[NEARBY_CHANNEL_ID] = new Channel(NEARBY_CHANNEL_ID, NEARBY_CHANNEL_ID, 0, 0, true, false,
                NEARBY_CHANNEL_DESCRIPTION);
        }

        public void Dispose()
        {
            operationsCancellationToken.Cancel();
            operationsCancellationToken.Dispose();

            apiBridge.OnInitialized -= Initialize;
            apiBridge.OnAddMessage -= AddMessages;
            apiBridge.OnTotalUnseenMessagesChanged -= UpdateTotalUnseenMessages;
            apiBridge.OnUserUnseenMessagesChanged -= UpdateTotalUnseenMessagesByUser;
            apiBridge.OnChannelUnseenMessagesChanged -= UpdateTotalUnseenMessagesByChannel;
            apiBridge.OnChannelMembersUpdated -= UpdateChannelMembers;
            apiBridge.OnChannelJoined -= JoinIntoChannel;
            apiBridge.OnChannelJoinFailed -= JoinChannelFailed;
            apiBridge.OnChannelLeaveFailed -= LeaveChannelFailed;
            apiBridge.OnChannelsUpdated -= UpdateChannelInfo;
            apiBridge.OnMuteChannelFailed -= MuteChannelFailed;
            apiBridge.OnChannelSearchResults -= UpdateChannelSearchResults;
        }

        public void Initialize()
        {
            apiBridge.OnInitialized += Initialize;
            apiBridge.OnAddMessage += AddMessages;
            apiBridge.OnTotalUnseenMessagesChanged += UpdateTotalUnseenMessages;
            apiBridge.OnUserUnseenMessagesChanged += UpdateTotalUnseenMessagesByUser;
            apiBridge.OnChannelUnseenMessagesChanged += UpdateTotalUnseenMessagesByChannel;
            apiBridge.OnChannelMembersUpdated += UpdateChannelMembers;
            apiBridge.OnChannelJoined += JoinIntoChannel;
            apiBridge.OnChannelJoinFailed += JoinChannelFailed;
            apiBridge.OnChannelLeaveFailed += LeaveChannelFailed;
            apiBridge.OnChannelsUpdated += UpdateChannelInfo;
            apiBridge.OnMuteChannelFailed += MuteChannelFailed;
            apiBridge.OnChannelSearchResults += UpdateChannelSearchResults;
        }

        private void Initialize(InitializeChatPayload msg)
        {
            if (chatAlreadyInitialized)
                return;

            totalUnseenMessages = msg.totalUnseenMessages;
            OnInitialized?.Invoke();
            OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
            chatAlreadyInitialized = true;

            if (!string.IsNullOrEmpty(msg.channelToJoin))
                OnAskForJoinChannel?.Invoke($"#{msg.channelToJoin.ToLower()}");
        }

        private void UpdateTotalUnseenMessages(UpdateTotalUnseenMessagesPayload msg)
        {
            totalUnseenMessages = msg.total;
            OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
        }

        private void UpdateTotalUnseenMessagesByUser((string userId, int count)[] userUnseenMessages)
        {
            foreach (var unseenMessages in userUnseenMessages)
            {
                string userId = unseenMessages.userId;
                int count = unseenMessages.count;
                unseenMessagesByUser[userId] = count;
                OnUserUnseenMessagesUpdated?.Invoke(userId, count);
            }
        }

        private void UpdateTotalUnseenMessagesByChannel((string channelId, int count)[] unseenChannelMessages)
        {
            foreach (var unseenMessages in unseenChannelMessages)
                UpdateTotalUnseenMessagesByChannel(unseenMessages.channelId, unseenMessages.count);
        }

        private void UpdateChannelMembers(UpdateChannelMembersPayload msg) =>
            OnUpdateChannelMembers?.Invoke(msg.channelId, msg.members);

        private void JoinIntoChannel(ChannelInfoPayloads msg)
        {
            if (msg.channelInfoPayload.Length == 0) return;

            foreach (var channelInfo in msg.channelInfoPayload)
            {
                Channel channel = channelInfo.ToChannel();
                string channelId = channel.ChannelId;

                if (channels.ContainsKey(channelId))
                    channels[channelId].CopyFrom(channel);
                else
                    channels[channelId] = channel;

                if (autoJoinChannelList.Contains(channelId))
                    OnAutoChannelJoined?.Invoke(channel);
                else
                    OnChannelJoined?.Invoke(channel);

                OnChannelUpdated?.Invoke(channel);
                autoJoinChannelList.Remove(channelId);

                SendChannelWelcomeMessage(channel);
            }

            // TODO (responsibility issues): extract to another class
            AudioScriptableObjects.joinChannel.Play(true);
        }

        private void JoinChannelFailed(JoinChannelErrorPayload msg)
        {
            OnJoinChannelError?.Invoke(msg.channelId, (ChannelErrorCode)msg.errorCode);
            autoJoinChannelList.Remove(msg.channelId);
        }

        private void LeaveChannelFailed(JoinChannelErrorPayload msg)
        {
            OnChannelLeaveError?.Invoke(msg.channelId, (ChannelErrorCode)msg.errorCode);
            autoJoinChannelList.Remove(msg.channelId);
        }

        private void UpdateChannelInfo(ChannelInfoPayloads msg)
        {
            var anyChannelLeft = false;

            foreach (var channelInfo in msg.channelInfoPayload)
            {
                string channelId = channelInfo.channelId;
                Channel channel = channelInfo.ToChannel();
                bool justLeft = !channel.Joined;

                if (channels.ContainsKey(channelId))
                {
                    justLeft = channels[channelId].Joined && !channel.Joined;
                    channels[channelId].CopyFrom(channel);
                }
                else
                    channels[channelId] = channel;

                if (justLeft)
                {
                    OnChannelLeft?.Invoke(channelId);
                    anyChannelLeft = true;
                }

                if (anyChannelLeft)
                {
                    // TODO (responsibility issues): extract to another class
                    AudioScriptableObjects.leaveChannel.Play(true);
                }

                OnChannelUpdated?.Invoke(channel);
            }
        }

        private void MuteChannelFailed(MuteChannelErrorPayload msg)
        {
            OnMuteChannelError?.Invoke(msg.channelId, (ChannelErrorCode)msg.errorCode);
        }

        private void UpdateChannelSearchResults(ChannelSearchResultsPayload msg)
        {
            var channelsResult = new Channel[msg.channels.Length];

            for (var i = 0; i < msg.channels.Length; i++)
            {
                ChannelInfoPayload channelPayload = msg.channels[i];
                string channelId = channelPayload.channelId;
                Channel channel = channelPayload.ToChannel();

                if (channels.ContainsKey(channelId))
                    channels[channelId].CopyFrom(channel);
                else
                    channels[channelId] = channel;

                channelsResult[i] = channel;
            }

            OnChannelSearchResult?.Invoke(msg.since, channelsResult);
        }

        public void LeaveChannel(string channelId)
        {
            apiBridge.LeaveChannel(channelId);
            autoJoinChannelList.Remove(channelId);
        }

        public async UniTask<Channel> JoinOrCreateChannelAsync(string channelId, CancellationToken cancellationToken = default)
        {
            CancellationTokenSource linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, operationsCancellationToken.Token);
            ChannelInfoPayload payload = await apiBridge.JoinOrCreateChannelAsync(channelId, linkedCancellationToken.Token);
            Channel channel = payload.ToChannel();
            channels[channel.ChannelId] = channel;
            OnChannelJoined?.Invoke(channel);
            return channel;
        }

        public void JoinOrCreateChannel(string channelId) =>
            apiBridge.JoinOrCreateChannel(channelId);

        public void GetChannelMessages(string channelId, int limit, string fromMessageId) =>
            apiBridge.GetChannelMessages(channelId, limit, fromMessageId);

        public void GetJoinedChannels(int limit, int skip) =>
            apiBridge.GetJoinedChannels(limit, skip);

        public async UniTask<(string, Channel[])> GetChannelsByNameAsync(int limit, string name, string paginationToken = null,
            CancellationToken cancellationToken = default)
        {
            CancellationTokenSource linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, operationsCancellationToken.Token);
            ChannelSearchResultsPayload searchResult = await apiBridge.GetChannelsAsync(limit, name, paginationToken, linkedCancellationToken.Token);
            return (searchResult.since, searchResult.channels.Select(payload => payload.ToChannel()).ToArray());
        }

        public void GetChannelsByName(int limit, string name, string paginationToken = null) =>
            apiBridge.GetChannels(limit, paginationToken, name);

        public void GetChannels(int limit, string paginationToken) =>
            apiBridge.GetChannels(limit, paginationToken, string.Empty);

        public void MuteChannel(string channelId)
        {
            if (channelId == NEARBY_CHANNEL_ID)
            {
                var channel = GetAllocatedChannel(NEARBY_CHANNEL_ID);

                var payload = new ChannelInfoPayloads
                {
                    channelInfoPayload = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = channel.Description,
                            joined = channel.Joined,
                            channelId = channel.ChannelId,
                            muted = true,
                            name = channel.Name,
                            memberCount = channel.MemberCount,
                            unseenMessages = channel.UnseenMessages
                        }
                    }
                };

                UpdateChannelInfo(payload);
            }
            else
                apiBridge.MuteChannel(channelId, true);
        }

        public void UnmuteChannel(string channelId)
        {
            if (channelId == NEARBY_CHANNEL_ID)
            {
                var channel = GetAllocatedChannel(NEARBY_CHANNEL_ID);

                var payload = new ChannelInfoPayloads
                {
                    channelInfoPayload = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = channel.Description,
                            joined = channel.Joined,
                            channelId = channel.ChannelId,
                            muted = false,
                            name = channel.Name,
                            memberCount = channel.MemberCount,
                            unseenMessages = channel.UnseenMessages
                        }
                    }
                };

                UpdateChannelInfo(payload);
            }
            else
                apiBridge.MuteChannel(channelId, false);
        }

        public Channel GetAllocatedChannel(string channelId) =>
            channels.ContainsKey(channelId) ? channels[channelId] : null;

        public Channel GetAllocatedChannelByName(string channelName) =>
            channels.Values.FirstOrDefault(x => x.Name == channelName);

        public void GetPrivateMessages(string userId, int limit, string fromMessageId) =>
            apiBridge.GetPrivateMessages(userId, limit, fromMessageId);

        public void MarkChannelMessagesAsSeen(string channelId)
        {
            if (channelId == NEARBY_CHANNEL_ID)
            {
                UpdateTotalUnseenMessagesByChannel(NEARBY_CHANNEL_ID, 0);
                OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
            }

            apiBridge.MarkChannelMessagesAsSeen(channelId);
        }

        public void GetUnseenMessagesByUser() =>
            apiBridge.GetUnseenMessagesByUser();

        public void GetUnseenMessagesByChannel() =>
            apiBridge.GetUnseenMessagesByChannel();

        public int GetAllocatedUnseenMessages(string userId) =>
            unseenMessagesByUser.ContainsKey(userId) ? unseenMessagesByUser[userId] : 0;

        public int GetAllocatedUnseenChannelMessages(string channelId) =>
            !string.IsNullOrEmpty(channelId)
                ? unseenMessagesByChannel.ContainsKey(channelId) ? unseenMessagesByChannel[channelId] : 0
                : 0;

        public void CreateChannel(string channelId) =>
            apiBridge.CreateChannel(channelId);

        public void GetChannelInfo(string[] channelIds) =>
            apiBridge.GetChannelInfo(channelIds);

        public void GetChannelMembers(string channelId, int limit, int skip, string name) =>
            apiBridge.GetChannelMembers(channelId, limit, skip, name);

        public void GetChannelMembers(string channelId, int limit, int skip) =>
            apiBridge.GetChannelMembers(channelId, limit, skip, string.Empty);

        public void Send(ChatMessage message) =>
            apiBridge.SendChatMessage(message);

        public void MarkMessagesAsSeen(string userId) =>
            apiBridge.MarkMessagesAsSeen(userId);

        private void SendChannelWelcomeMessage(Channel channel)
        {
            var message =
                new ChatMessage(ChatMessage.Type.SYSTEM, "", @$"This is the start of the channel #{channel.Name}.\n
Invite others to join by quoting the channel name in other chats or include it as a part of your bio.")
                {
                    recipient = channel.ChannelId,
                    timestamp = 0,
                    messageId = Guid.NewGuid().ToString()
                };

            AddMessages(new[] { message });
        }

        private void AddMessages(ChatMessage[] messages)
        {
            if (messages == null) return;

            var nearbyUpdated = false;

            var nearbyUnseenMessages = unseenMessagesByChannel.ContainsKey(NEARBY_CHANNEL_ID)
                ? unseenMessagesByChannel[NEARBY_CHANNEL_ID]
                : 0;

            foreach (var message in messages)
            {
                if (message.messageType != ChatMessage.Type.PUBLIC) continue;
                if (!string.IsNullOrEmpty(message.recipient)) continue;

                nearbyUnseenMessages++;
                nearbyUpdated = true;
            }

            if (nearbyUpdated)
            {
                UpdateTotalUnseenMessagesByChannel(NEARBY_CHANNEL_ID, nearbyUnseenMessages);
                OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
            }

            OnAddMessage?.Invoke(messages);
        }

        private void UpdateTotalUnseenMessagesByChannel(string channelId, int count)
        {
            unseenMessagesByChannel[channelId] = count;

            if (channels.ContainsKey(channelId))
                channels[channelId].UnseenMessages = count;

            OnChannelUnseenMessagesUpdated?.Invoke(channelId, count);
        }
    }
}
