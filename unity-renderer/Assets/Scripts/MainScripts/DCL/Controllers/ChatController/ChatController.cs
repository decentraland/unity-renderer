using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Chat.Channels;
using DCL.Chat.WebApi;
using DCL.Interface;
using JetBrains.Annotations;
using UnityEngine;

namespace DCL.Chat
{
    public partial class ChatController : MonoBehaviour, IChatController
    {
        private const string NEARBY_CHANNEL_DESCRIPTION =
            "Talk to the people around you. If you move far away from someone you will lose contact. All whispers will be displayed.";

        private const string NEARBY_CHANNEL_ID = "nearby";

        public static ChatController i { get; private set; }

        private readonly Dictionary<string, int> unseenMessagesByUser = new Dictionary<string, int>();
        private readonly Dictionary<string, int> unseenMessagesByChannel = new Dictionary<string, int>();
        private readonly Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
        private readonly List<ChatMessage> messages = new List<ChatMessage>();
        private bool chatAlreadyInitialized;
        private int totalUnseenMessages;

        public event Action<Channel> OnChannelUpdated;
        public event Action<Channel> OnChannelJoined;
        public event Action<string, ChannelErrorCode> OnJoinChannelError;
        public event Action<string> OnChannelLeft;
        public event Action<string, ChannelErrorCode> OnChannelLeaveError;
        public event Action<string, ChannelErrorCode> OnMuteChannelError;
        public event Action OnInitialized;
        public event Action<ChatMessage> OnAddMessage;
        public event Action<int> OnTotalUnseenMessagesUpdated;
        public event Action<string, int> OnUserUnseenMessagesUpdated;
        public event Action<string, ChannelMember[]> OnUpdateChannelMembers;
        public event Action<string, Channel[]> OnChannelSearchResult;
        public event Action<string, int> OnChannelUnseenMessagesUpdated;

        // since kernel does not calculate the #nearby channel unseen messages, it is handled on renderer side
        public int TotalUnseenMessages => totalUnseenMessages
                                          + (unseenMessagesByChannel.ContainsKey(NEARBY_CHANNEL_ID)
                                              ? unseenMessagesByChannel[NEARBY_CHANNEL_ID]
                                              : 0);

        public void Awake()
        {
            i = this;

            channels[NEARBY_CHANNEL_ID] = new Channel(NEARBY_CHANNEL_ID, NEARBY_CHANNEL_ID, 0, 0, true, false,
                NEARBY_CHANNEL_DESCRIPTION);
        }

        // called by kernel
        [PublicAPI]
        public void InitializeChat(string json)
        {
            if (chatAlreadyInitialized)
                return;

            var msg = JsonUtility.FromJson<InitializeChatPayload>(json);

            totalUnseenMessages = msg.totalUnseenMessages;
            OnInitialized?.Invoke();
            OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
            chatAlreadyInitialized = true;
        }

        // called by kernel
        [PublicAPI]
        public void AddMessageToChatWindow(string jsonMessage) =>
            AddMessage(JsonUtility.FromJson<ChatMessage>(jsonMessage));

        // called by kernel
        [PublicAPI]
        public void AddChatMessages(string jsonMessage)
        {
            var messages = JsonUtility.FromJson<ChatMessageListPayload>(jsonMessage);

            if (messages == null) return;

            foreach (var message in messages.messages)
                AddMessage(message);
        }

        // called by kernel
        [PublicAPI]
        public void UpdateTotalUnseenMessages(string json)
        {
            var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesPayload>(json);
            totalUnseenMessages = msg.total;
            OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
        }

        // called by kernel
        [PublicAPI]
        public void UpdateUserUnseenMessages(string json)
        {
            var msg = JsonUtility.FromJson<UpdateUserUnseenMessagesPayload>(json);
            unseenMessagesByUser[msg.userId] = msg.total;
            OnUserUnseenMessagesUpdated?.Invoke(msg.userId, msg.total);
        }

        // called by kernel
        [PublicAPI]
        public void UpdateTotalUnseenMessagesByUser(string json)
        {
            var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesByUserPayload>(json);

            foreach (var unseenMessages in msg.unseenPrivateMessages)
            {
                var userId = unseenMessages.userId;
                var count = unseenMessages.count;
                unseenMessagesByUser[userId] = count;
                OnUserUnseenMessagesUpdated?.Invoke(userId, count);
            }
        }

        // called by kernel
        [PublicAPI]
        public void UpdateTotalUnseenMessagesByChannel(string json)
        {
            var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesByChannelPayload>(json);
            foreach (var unseenMessages in msg.unseenChannelMessages)
                UpdateTotalUnseenMessagesByChannel(unseenMessages.channelId, unseenMessages.count);
        }

        // called by kernel
        [PublicAPI]
        public void UpdateChannelMembers(string payload)
        {
            var msg = JsonUtility.FromJson<UpdateChannelMembersPayload>(payload);
            OnUpdateChannelMembers?.Invoke(msg.channelId, msg.members);
        }

        [PublicAPI]
        public void UpdateChannelInfo(string payload)
        {
            var msg = JsonUtility.FromJson<ChannelInfoPayloads>(payload);
            var anyChannelLeft = false;

            foreach (var channelInfo in msg.channelInfoPayload)
            {
                var channelId = channelInfo.channelId;
                var channel = new Channel(channelId, channelInfo.name, channelInfo.unseenMessages, channelInfo.memberCount,
                    channelInfo.joined, channelInfo.muted, channelInfo.description);
                var justLeft = !channel.Joined;

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

        // called by kernel
        [PublicAPI]
        public void JoinChannelConfirmation(string payload)
        {
            var msg = JsonUtility.FromJson<ChannelInfoPayloads>(payload);

            if (msg.channelInfoPayload.Length == 0)
                return;

            var channelInfo = msg.channelInfoPayload[0];
            var channel = new Channel(channelInfo.channelId, channelInfo.name, channelInfo.unseenMessages,
                channelInfo.memberCount, channelInfo.joined, channelInfo.muted, channelInfo.description);
            var channelId = channel.ChannelId;

            if (channels.ContainsKey(channelId))
                channels[channelId].CopyFrom(channel);
            else
                channels[channelId] = channel;

            OnChannelJoined?.Invoke(channel);
            OnChannelUpdated?.Invoke(channel);

            // TODO (responsibility issues): extract to another class
            AudioScriptableObjects.joinChannel.Play(true);

            SendChannelWelcomeMessage(channel);
        }

        // called by kernel
        [PublicAPI]
        public void JoinChannelError(string payload)
        {
            var msg = JsonUtility.FromJson<JoinChannelErrorPayload>(payload);
            OnJoinChannelError?.Invoke(msg.channelId, (ChannelErrorCode) msg.errorCode);
        }

        // called by kernel
        [PublicAPI]
        public void LeaveChannelError(string payload)
        {
            var msg = JsonUtility.FromJson<JoinChannelErrorPayload>(payload);
            OnChannelLeaveError?.Invoke(msg.channelId, (ChannelErrorCode) msg.errorCode);
        }

        // called by kernel
        [PublicAPI]
        public void MuteChannelError(string payload)
        {
            var msg = JsonUtility.FromJson<MuteChannelErrorPayload>(payload);
            OnMuteChannelError?.Invoke(msg.channelId, (ChannelErrorCode) msg.errorCode);
        }

        // called by kernel
        [PublicAPI]
        public void UpdateChannelSearchResults(string payload)
        {
            var msg = JsonUtility.FromJson<ChannelSearchResultsPayload>(payload);
            var channelsResult = new Channel[msg.channels.Length];

            for (var i = 0; i < msg.channels.Length; i++)
            {
                var channelPayload = msg.channels[i];
                var channelId = channelPayload.channelId;
                var channel = new Channel(channelId, channelPayload.name, channelPayload.unseenMessages,
                    channelPayload.memberCount,
                    channelPayload.joined, channelPayload.muted, channelPayload.description);

                if (channels.ContainsKey(channelId))
                    channels[channelId].CopyFrom(channel);
                else
                    channels[channelId] = channel;

                channelsResult[i] = channel;
            }

            OnChannelSearchResult?.Invoke(msg.since, channelsResult);
        }

        public void JoinOrCreateChannel(string channelId) => WebInterface.JoinOrCreateChannel(channelId);

        public void LeaveChannel(string channelId) => WebInterface.LeaveChannel(channelId);

        public void GetChannelMessages(string channelId, int limit, string fromMessageId) =>
            WebInterface.GetChannelMessages(channelId, limit, fromMessageId);

        public void GetJoinedChannels(int limit, int skip) => WebInterface.GetJoinedChannels(limit, skip);

        public void GetChannelsByName(int limit, string name, string paginationToken = null) =>
            WebInterface.GetChannels(limit, paginationToken, name);

        public void GetChannels(int limit, string paginationToken) =>
            WebInterface.GetChannels(limit, paginationToken, string.Empty);

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

                UpdateChannelInfo(JsonUtility.ToJson(payload));
            }
            else
                WebInterface.MuteChannel(channelId, true);
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

                UpdateChannelInfo(JsonUtility.ToJson(payload));
            }
            else
                WebInterface.MuteChannel(channelId, false);
        }

        public Channel GetAllocatedChannel(string channelId) =>
            channels.ContainsKey(channelId) ? channels[channelId] : null;

        public Channel GetAllocatedChannelByName(string channelName) =>
            channels.Values.FirstOrDefault(x => x.Name == channelName);

        public void GetPrivateMessages(string userId, int limit, string fromMessageId) =>
            WebInterface.GetPrivateMessages(userId, limit, fromMessageId);

        public void MarkChannelMessagesAsSeen(string channelId)
        {
            if (channelId == NEARBY_CHANNEL_ID)
            {
                UpdateTotalUnseenMessagesByChannel(NEARBY_CHANNEL_ID, 0);
                OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
            }

            WebInterface.MarkChannelMessagesAsSeen(channelId);
        }

        public List<ChatMessage> GetAllocatedEntries() => new List<ChatMessage>(messages);

        public void GetUnseenMessagesByUser() => WebInterface.GetUnseenMessagesByUser();

        public void GetUnseenMessagesByChannel() => WebInterface.GetUnseenMessagesByChannel();

        public int GetAllocatedUnseenMessages(string userId) =>
            unseenMessagesByUser.ContainsKey(userId) ? unseenMessagesByUser[userId] : 0;

        public int GetAllocatedUnseenChannelMessages(string channelId) =>
            !string.IsNullOrEmpty(channelId)
                ? unseenMessagesByChannel.ContainsKey(channelId) ? unseenMessagesByChannel[channelId] : 0
                : 0;

        public void CreateChannel(string channelId) => WebInterface.CreateChannel(channelId);

        public List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId)
        {
            return messages
                .Where(x => (x.sender == userId || x.recipient == userId) && x.messageType == ChatMessage.Type.PRIVATE)
                .ToList();
        }

        public void GetChannelInfo(string[] channelIds) => WebInterface.GetChannelInfo(channelIds);

        public void GetChannelMembers(string channelId, int limit, int skip, string name) =>
            WebInterface.GetChannelMembers(channelId, limit, skip, name);

        public void GetChannelMembers(string channelId, int limit, int skip) =>
            WebInterface.GetChannelMembers(channelId, limit, skip, string.Empty);

        public void Send(ChatMessage message) => WebInterface.SendChatMessage(message);

        public void MarkMessagesAsSeen(string userId) => WebInterface.MarkMessagesAsSeen(userId);

        private void SendChannelWelcomeMessage(Channel channel)
        {
            var message =
                new ChatMessage(ChatMessage.Type.SYSTEM, "", @$"This is the start of the channel #{channel.Name}.\n
Invite others to join by quoting the channel name in other chats or include it as a part of your bio.")
                {
                    recipient = channel.ChannelId,
                    timestamp = 0,
                    isChannelMessage = true,
                    messageId = Guid.NewGuid().ToString()
                };

            AddMessage(message);
        }

        private void AddMessage(ChatMessage message)
        {
            if (message == null) return;

            messages.Add(message);

            if (message.messageType == ChatMessage.Type.PUBLIC && string.IsNullOrEmpty(message.recipient))
            {
                if (!unseenMessagesByChannel.ContainsKey(NEARBY_CHANNEL_ID))
                    unseenMessagesByChannel[NEARBY_CHANNEL_ID] = 0;

                UpdateTotalUnseenMessagesByChannel(NEARBY_CHANNEL_ID, unseenMessagesByChannel[NEARBY_CHANNEL_ID] + 1);
                OnTotalUnseenMessagesUpdated?.Invoke(TotalUnseenMessages);
            }

            OnAddMessage?.Invoke(message);
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