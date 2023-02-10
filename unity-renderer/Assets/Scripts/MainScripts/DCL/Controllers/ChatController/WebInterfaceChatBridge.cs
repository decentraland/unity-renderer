using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using DCL.Chat.Channels;
using DCL.Chat.WebApi;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class WebInterfaceChatBridge : MonoBehaviour, IChatApiBridge
    {
        private const string JOIN_OR_CREATE_CHANNEL_TASK_ID = "JoinOrCreateChannelAsync";
        private const string GET_CHANNELS_TASK_ID = "GetChannelsAsync";
        private const double REQUEST_TIMEOUT_SECONDS = 30;

        public static IChatApiBridge GetOrCreate()
        {
            var bridgeObj = SceneReferences.i?.bridgeGameObject;
            return bridgeObj == null
                ? new GameObject("Bridges").AddComponent<WebInterfaceChatBridge>()
                : bridgeObj.GetOrCreateComponent<WebInterfaceChatBridge>();
        }

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

        private readonly Dictionary<string, IUniTaskSource> pendingTasks = new ();

        [PublicAPI]
        public void InitializeChat(string json) =>
            OnInitialized?.Invoke(JsonUtility.FromJson<InitializeChatPayload>(json));

        [PublicAPI]
        public void AddMessageToChatWindow(string jsonMessage) =>
            OnAddMessage?.Invoke(new[] { JsonUtility.FromJson<ChatMessage>(jsonMessage) });

        [PublicAPI]
        public void AddChatMessages(string jsonMessage)
        {
            var msg = JsonUtility.FromJson<ChatMessageListPayload>(jsonMessage);
            if (msg == null) return;
            OnAddMessage?.Invoke(msg.messages);
        }

        [PublicAPI]
        public void UpdateTotalUnseenMessages(string json) =>
            OnTotalUnseenMessagesChanged?.Invoke(JsonUtility.FromJson<UpdateTotalUnseenMessagesPayload>(json));

        [PublicAPI]
        public void UpdateUserUnseenMessages(string json)
        {
            var msg = JsonUtility.FromJson<UpdateUserUnseenMessagesPayload>(json);
            OnUserUnseenMessagesChanged?.Invoke(new[] { (msg.userId, msg.total) });
        }

        [PublicAPI]
        public void UpdateTotalUnseenMessagesByUser(string json)
        {
            var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesByUserPayload>(json);

            OnUserUnseenMessagesChanged?.Invoke(msg.unseenPrivateMessages
                                                   .Select(unseenMessages => (unseenMessages.userId, unseenMessages.count))
                                                   .ToArray());
        }

        [PublicAPI]
        public void UpdateTotalUnseenMessagesByChannel(string json)
        {
            var msg = JsonUtility.FromJson<UpdateTotalUnseenMessagesByChannelPayload>(json);

            OnChannelUnseenMessagesChanged?.Invoke(msg.unseenChannelMessages
                                                      .Select(message => (message.channelId, message.count))
                                                      .ToArray());
        }

        [PublicAPI]
        public void UpdateChannelMembers(string json) =>
            OnChannelMembersUpdated?.Invoke(JsonUtility.FromJson<UpdateChannelMembersPayload>(json));

        [PublicAPI]
        public void JoinChannelConfirmation(string payload)
        {
            ChannelInfoPayloads payloads = JsonUtility.FromJson<ChannelInfoPayloads>(payload);

            if (pendingTasks.TryGetValue(JOIN_OR_CREATE_CHANNEL_TASK_ID, out var task))
            {
                pendingTasks.Remove(JOIN_OR_CREATE_CHANNEL_TASK_ID);
                UniTaskCompletionSource<ChannelInfoPayload> taskCompletionSource = (UniTaskCompletionSource<ChannelInfoPayload>)task;
                taskCompletionSource.TrySetResult(payloads.channelInfoPayload[0]);
            }

            OnChannelJoined?.Invoke(payloads);
        }

        [PublicAPI]
        public void JoinChannelError(string payload)
        {
            JoinChannelErrorPayload errorPayload = JsonUtility.FromJson<JoinChannelErrorPayload>(payload);

            if (pendingTasks.TryGetValue(JOIN_OR_CREATE_CHANNEL_TASK_ID, out var task))
            {
                pendingTasks.Remove(JOIN_OR_CREATE_CHANNEL_TASK_ID);
                UniTaskCompletionSource<ChannelInfoPayload> taskCompletionSource = (UniTaskCompletionSource<ChannelInfoPayload>)task;
                taskCompletionSource.TrySetException(new ChannelException(errorPayload.channelId, (ChannelErrorCode) errorPayload.errorCode));
            }

            OnChannelJoinFailed?.Invoke(errorPayload);
        }

        [PublicAPI]
        public void LeaveChannelError(string payload) =>
            OnChannelLeaveFailed?.Invoke(JsonUtility.FromJson<JoinChannelErrorPayload>(payload));

        [PublicAPI]
        public void UpdateChannelInfo(string payload) =>
            OnChannelsUpdated?.Invoke(JsonUtility.FromJson<ChannelInfoPayloads>(payload));

        [PublicAPI]
        public void MuteChannelError(string payload) =>
            OnMuteChannelFailed?.Invoke(JsonUtility.FromJson<MuteChannelErrorPayload>(payload));

        [PublicAPI]
        public void UpdateChannelSearchResults(string payload)
        {
            ChannelSearchResultsPayload searchResultPayload = JsonUtility.FromJson<ChannelSearchResultsPayload>(payload);

            if (pendingTasks.TryGetValue(GET_CHANNELS_TASK_ID, out var task))
            {
                pendingTasks.Remove(GET_CHANNELS_TASK_ID);
                UniTaskCompletionSource<ChannelSearchResultsPayload> taskCompletionSource = (UniTaskCompletionSource<ChannelSearchResultsPayload>)task;
                taskCompletionSource.TrySetResult(searchResultPayload);
            }

            OnChannelSearchResults?.Invoke(searchResultPayload);
        }

        public void LeaveChannel(string channelId) =>
            WebInterface.LeaveChannel(channelId);

        public void GetChannelMessages(string channelId, int limit, string fromMessageId) =>
            WebInterface.GetChannelMessages(channelId, limit, fromMessageId);

        public UniTask<ChannelInfoPayload> JoinOrCreateChannelAsync(string channelId, CancellationToken cancellationToken = default)
        {
            if (pendingTasks.ContainsKey(JOIN_OR_CREATE_CHANNEL_TASK_ID))
            {
                UniTaskCompletionSource<ChannelInfoPayload> pendingTask = (UniTaskCompletionSource<ChannelInfoPayload>)pendingTasks[JOIN_OR_CREATE_CHANNEL_TASK_ID];
                cancellationToken.RegisterWithoutCaptureExecutionContext(() => pendingTask.TrySetCanceled());
                return pendingTask.Task;
            }

            UniTaskCompletionSource<ChannelInfoPayload> task = new ();
            pendingTasks[JOIN_OR_CREATE_CHANNEL_TASK_ID] = task;
            cancellationToken.RegisterWithoutCaptureExecutionContext(() => task.TrySetCanceled());

            WebInterface.JoinOrCreateChannel(channelId);

            return task.Task.Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT_SECONDS));
        }

        public void JoinOrCreateChannel(string channelId) =>
            WebInterface.JoinOrCreateChannel(channelId);

        public void GetJoinedChannels(int limit, int skip) =>
            WebInterface.GetJoinedChannels(limit, skip);

        public void GetChannels(int limit, string paginationToken, string name) =>
            WebInterface.GetChannels(limit, paginationToken, name);

        public UniTask<ChannelSearchResultsPayload> GetChannelsAsync(int limit, string name, string paginationToken, CancellationToken cancellationToken = default)
        {
            if (pendingTasks.ContainsKey(GET_CHANNELS_TASK_ID))
            {
                UniTaskCompletionSource<ChannelSearchResultsPayload> pendingTask = (UniTaskCompletionSource<ChannelSearchResultsPayload>)pendingTasks[GET_CHANNELS_TASK_ID];
                cancellationToken.RegisterWithoutCaptureExecutionContext(() => pendingTask.TrySetCanceled());
                return pendingTask.Task;
            }

            UniTaskCompletionSource<ChannelSearchResultsPayload> task = new ();
            pendingTasks[GET_CHANNELS_TASK_ID] = task;
            cancellationToken.RegisterWithoutCaptureExecutionContext(() => task.TrySetCanceled());

            WebInterface.GetChannels(limit, paginationToken, name);

            return task.Task.Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT_SECONDS));
        }

        public void MuteChannel(string channelId, bool muted) =>
            WebInterface.MuteChannel(channelId, muted);

        public void GetPrivateMessages(string userId, int limit, string fromMessageId) =>
            WebInterface.GetPrivateMessages(userId, limit, fromMessageId);

        public void MarkChannelMessagesAsSeen(string channelId) =>
            WebInterface.MarkChannelMessagesAsSeen(channelId);

        public void GetUnseenMessagesByUser() =>
            WebInterface.GetUnseenMessagesByUser();

        public void GetUnseenMessagesByChannel() =>
            WebInterface.GetUnseenMessagesByChannel();

        public void CreateChannel(string channelId) =>
            WebInterface.CreateChannel(channelId);

        public void GetChannelInfo(string[] channelIds) =>
            WebInterface.GetChannelInfo(channelIds);

        public void GetChannelMembers(string channelId, int limit, int skip, string name) =>
            WebInterface.GetChannelMembers(channelId, limit, skip, name);

        public void SendChatMessage(ChatMessage message) =>
            WebInterface.SendChatMessage(message);

        public void MarkMessagesAsSeen(string userId) =>
            WebInterface.MarkMessagesAsSeen(userId);
    }
}
