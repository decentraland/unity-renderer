using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL.Chat.WebApi;
using DCL.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DCL.Chat.Channels
{
    public class ChatChannelsControllerMock : IChatController
    {
        private readonly ChatController controller;
        private readonly List<string> joinedChannels = new List<string>();

        private static ChatChannelsControllerMock sharedInstance; 
        public static ChatChannelsControllerMock i => sharedInstance ??= new ChatChannelsControllerMock(ChatController.i);

        public event Action<int> OnTotalUnseenMessagesUpdated
        {
            add => controller.OnTotalUnseenMessagesUpdated += value;
            remove => controller.OnTotalUnseenMessagesUpdated -= value;
        }

        public event Action<string, int> OnUserUnseenMessagesUpdated
        {
            add => controller.OnUserUnseenMessagesUpdated += value;
            remove => controller.OnUserUnseenMessagesUpdated -= value;
        }

        public event Action<string, int> OnChannelUnseenMessagesUpdated
        {
            add => controller.OnChannelUnseenMessagesUpdated += value;
            remove => controller.OnChannelUnseenMessagesUpdated -= value;
        }

        public event Action<ChatMessage> OnAddMessage
        {
            add => controller.OnAddMessage += value;
            remove => controller.OnAddMessage -= value;
        }

        public event Action<Channel> OnChannelUpdated
        {
            add => controller.OnChannelUpdated += value;
            remove => controller.OnChannelUpdated -= value;
        }

        public event Action<Channel> OnChannelJoined
        {
            add => controller.OnChannelJoined += value;
            remove => controller.OnChannelJoined -= value;
        }

        public event Action<string, string> OnJoinChannelError
        {
            add => controller.OnJoinChannelError += value;
            remove => controller.OnJoinChannelError -= value;
        }

        public event Action<string> OnChannelLeft
        {
            add => controller.OnChannelLeft += value;
            remove => controller.OnChannelLeft -= value;
        }

        public event Action<string, string> OnChannelLeaveError
        {
            add => controller.OnChannelLeaveError += value;
            remove => controller.OnChannelLeaveError -= value;
        }

        public event Action<string, string> OnMuteChannelError
        {
            add => controller.OnMuteChannelError += value;
            remove => controller.OnMuteChannelError -= value;
        }

        public int TotalJoinedChannelCount => 5;

        public int TotalUnseenMessages => controller.TotalUnseenMessages;

        public ChatChannelsControllerMock(
            ChatController controller)
        {
            this.controller = controller;

            SimulateDelayedResponseFor_ChatInitialization().Forget();
        }

        public List<ChatMessage> GetAllocatedEntries() => controller.GetAllocatedEntries();

        public List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId) =>
            controller.GetPrivateAllocatedEntriesByUser(userId);

        public void GetUnseenMessagesByChannel() => SimulateDelayedResponseFor_TotalUnseenMessagesByChannel().Forget();

        public void Send(ChatMessage message)
        {
            if (IsChannelMessage(message))
            {
                // simulate a response since the real controller does not support channel messages yet
                controller.AddMessageToChatWindow(JsonUtility.ToJson(message));
            }
            else
                controller.Send(message);

            SimulateDelayedResponseFor_JoinChatMessage(message.body).Forget();
        }

        private bool IsChannelMessage(ChatMessage message)
        {
            return !string.IsNullOrEmpty(message.recipient)
                   && GetAllocatedChannel(message.recipient) != null;
        }

        private async UniTask SimulateDelayedResponseFor_JoinChatMessage(string chatMessage)
        {
            await UniTask.Delay(Random.Range(500, 1000));

            string chatMessagerToLower = chatMessage.ToLower();

            if (chatMessagerToLower.StartsWith("/join "))
            {
                string channelId = chatMessagerToLower.Split(' ')[1].Replace("#", "");

                if (!chatMessagerToLower.Contains("error"))
                {
                    if (!joinedChannels.Contains(channelId))
                        joinedChannels.Add(channelId);
                    
                    controller.JoinChannelConfirmation(CreateMockedDataFor_ChannelInfoPayload(channelId));
                }
                else
                    controller.JoinChannelError(CreateMockedDataFor_JoinChannelErrorPayload(channelId));
            }
        }

        private string CreateMockedDataFor_ChannelInfoPayload(string channelId)
        {
            ChannelInfoPayload mockedPayload = new ChannelInfoPayload
            {
                joined = true,
                channelId = channelId,
                muted = false,
                memberCount = Random.Range(0, 16),
                unseenMessages = Random.Range(0, 16),
                description = Random.Range(0, 2) == 0 ? "" : "This is a test description for the channel. This will be used to describe the main purpose of the channel."
            };

            return JsonUtility.ToJson(mockedPayload);
        }

        private string CreateMockedDataFor_JoinChannelErrorPayload(string joinMessage)
        {
            JoinChannelErrorPayload mockedPayload = new JoinChannelErrorPayload
            {
                channelId = joinMessage.Split(' ')[1].Replace("#", ""),
                message = "There was an error creating the channel."
            };

            return JsonUtility.ToJson(mockedPayload);
        }

        public void MarkMessagesAsSeen(string userId) => controller.MarkMessagesAsSeen(userId);

        public void MarkChannelMessagesAsSeen(string channelId)
        {
            controller.MarkChannelMessagesAsSeen(channelId);

            SimulateDelayedResponseFor_MarkChannelAsSeen(channelId).Forget();
        }

        public void GetPrivateMessages(string userId, int limit, string fromMessageId) =>
            controller.GetPrivateMessages(userId, limit, fromMessageId);

        public void JoinOrCreateChannel(string channelId) =>
            SimulateDelayedResponseFor_JoinOrCreateChannel(channelId).Forget();

        private async UniTask SimulateDelayedResponseFor_JoinOrCreateChannel(string channelId)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            if (!joinedChannels.Contains(channelId))
                joinedChannels.Add(channelId);
            
            controller.JoinChannelConfirmation(CreateMockedDataFor_ChannelInfoPayload(channelId));
        }

        public void LeaveChannel(string channelId) => LeaveFakeChannel(channelId).Forget();

        private async UniTask LeaveFakeChannel(string channelId)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            var msg = new ChannelInfoPayload
            {
                joined = false,
                channelId = channelId,
                muted = false,
                memberCount = Random.Range(0, 16),
                unseenMessages = 0
            };
            joinedChannels.Remove(channelId);
            controller.UpdateChannelInfo(JsonUtility.ToJson(msg));
        }

        public void GetChannelMessages(string channelId, int limit, long fromTimestamp)
        {
        }

        public void GetJoinedChannels(int limit, int skip)
        {
            for (var i = skip; i < skip + limit && i < joinedChannels.Count; i++)
            {
                var channelId = joinedChannels[i];
                
                var msg = new ChannelInfoPayload
                {
                    joined = true,
                    channelId = channelId,
                    muted = false,
                    memberCount = Random.Range(0, 16),
                    unseenMessages = 0
                };
                
                controller.UpdateChannelInfo(JsonUtility.ToJson(msg));
            }
        }

        public void GetChannels(int limit, int skip, string name) =>
            GetFakeChannels(limit, skip, name).Forget();

        public void GetChannels(int limit, int skip) =>
            GetFakeChannels(limit, skip, "").Forget();

        private async UniTask GetFakeChannels(int limit, int skip, string name)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            var ids = new []
            {
                "help",
                "global",
                "argentina",
                "spain",
                "trade",
                "ice-poker",
                "dcl-sdk",
                "btc",
                "eth",
                "nfts",
                "lands",
                "art-week",
                "music-festival"
            };

            for (var i = skip; i < skip + limit && i < ids.Length; i++)
            {
                var channelId = ids[i];
                if (!channelId.StartsWith(name) && !string.IsNullOrEmpty(name)) continue;

                var msg = new ChannelInfoPayload
                {
                    joined = joinedChannels.Contains(channelId),
                    channelId = channelId,
                    muted = false,
                    memberCount = Random.Range(0, 16),
                    unseenMessages = 0
                };
                controller.UpdateChannelInfo(JsonUtility.ToJson(msg));
            }
        }

        public void MuteChannel(string channelId) => MuteFakeChannel(channelId).Forget();

        public Channel GetAllocatedChannel(string channelId) => controller.GetAllocatedChannel(channelId);

        public List<ChatMessage> GetAllocatedEntriesByChannel(string channelId) =>
            controller.GetAllocatedEntriesByChannel(channelId);

        public void GetUnseenMessagesByUser() => controller.GetUnseenMessagesByUser();

        public int GetAllocatedUnseenMessages(string userId) => controller.GetAllocatedUnseenMessages(userId);

        public int GetAllocatedUnseenChannelMessages(string channelId) => controller.GetAllocatedUnseenChannelMessages(channelId);
        
        public void CreateChannel(string channelId)
        {
            SimulateDelayedResponseFor_CreateChannel(channelId).Forget();
        }

        private async UniTask SimulateDelayedResponseFor_CreateChannel(string channelId)
        {
            await UniTask.Delay(Random.Range(40, 1000));
            
            if (!joinedChannels.Contains(channelId))
                joinedChannels.Add(channelId);
            
            controller.JoinChannelConfirmation(JsonUtility.ToJson(new ChannelInfoPayload
            {
                description = "",
                channelId = channelId,
                joined = true,
                memberCount = 1,
                muted = false,
                unseenMessages = 0
            }));
        }

        private async UniTask MuteFakeChannel(string channelId)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            var msg = new ChannelInfoPayload
            {
                joined = true,
                channelId = channelId,
                muted = true,
                memberCount = Random.Range(0, 16),
                unseenMessages = Random.Range(0, 16)
            };
            controller.UpdateChannelInfo(JsonUtility.ToJson(msg));
        }

        private async UniTask SimulateDelayedResponseFor_ChatInitialization()
        {
            await UniTask.Delay(Random.Range(50, 1000));

            var payload = new InitializeChatPayload
            {
                totalUnseenMessages = Random.Range(0, 5)
            };
            controller.InitializeChat(JsonUtility.ToJson(payload));
        }

        private async UniTask SimulateDelayedResponseFor_MarkChannelAsSeen(string channelId)
        {
            await UniTask.Delay(Random.Range(50, 1000));

            var totalPayload = new UpdateTotalUnseenMessagesPayload
            {
                total = Random.Range(0, 10)
            };
            controller.UpdateTotalUnseenMessages(JsonUtility.ToJson(totalPayload));

            var userPayload = new UpdateTotalUnseenMessagesByChannelPayload
            {
                unseenChannelMessages = new[]
                {
                    new UpdateTotalUnseenMessagesByChannelPayload.UnseenChannelMessage 
                    { 
                        channelId = channelId,
                        count = 0
                    }
                }
            };
            controller.UpdateTotalUnseenMessagesByChannel(JsonUtility.ToJson(userPayload));
        }

        private async UniTask SimulateDelayedResponseFor_TotalUnseenMessagesByChannel()
        {
            await UniTask.Delay(1500);

            var unseenChannelMessages = Enumerable.Range(0, joinedChannels.Count + 1)
                .Select(i => new UpdateTotalUnseenMessagesByChannelPayload.UnseenChannelMessage
                {
                    count = Random.Range(0, 10),
                    channelId = i < joinedChannels.Count ? joinedChannels[i] : "nearby"
                }).ToArray();

            var payload = new UpdateTotalUnseenMessagesByChannelPayload
            {
                unseenChannelMessages = unseenChannelMessages
            };

            controller.UpdateTotalUnseenMessagesByChannel(JsonUtility.ToJson(payload));
        }
    }
}