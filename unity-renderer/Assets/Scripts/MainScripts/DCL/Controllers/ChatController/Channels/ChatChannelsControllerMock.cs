using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Chat.WebApi;
using DCL.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DCL.Chat.Channels
{
    public class ChatChannelsControllerMock : IChatController
    {
        private const int CHANNEL_LIMIT = 5;
        private readonly ChatController controller;
        private readonly UserProfileController userProfileController;
        private readonly List<string> joinedChannels = new List<string>();
        private string currentChannelId;

        private static ChatChannelsControllerMock sharedInstance;

        public static ChatChannelsControllerMock i =>
            sharedInstance ??= new ChatChannelsControllerMock(ChatController.i, UserProfileController.i);

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

        public event Action OnInitialized
        {
            add => controller.OnInitialized += value;
            remove => controller.OnInitialized -= value;
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

        public event Action<string, ChannelErrorCode> OnJoinChannelError
        {
            add => controller.OnJoinChannelError += value;
            remove => controller.OnJoinChannelError -= value;
        }

        public event Action<string> OnChannelLeft
        {
            add => controller.OnChannelLeft += value;
            remove => controller.OnChannelLeft -= value;
        }

        public event Action<string, ChannelErrorCode> OnChannelLeaveError
        {
            add => controller.OnChannelLeaveError += value;
            remove => controller.OnChannelLeaveError -= value;
        }

        public event Action<string, ChannelErrorCode> OnMuteChannelError
        {
            add => controller.OnMuteChannelError += value;
            remove => controller.OnMuteChannelError -= value;
        }

        public event Action<string, ChannelMember[]> OnUpdateChannelMembers
        {
            add => controller.OnUpdateChannelMembers += value;
            remove => controller.OnUpdateChannelMembers -= value;
        }

        public event Action<string, Channel[]> OnChannelSearchResult
        {
            add => controller.OnChannelSearchResult += value;
            remove => controller.OnChannelSearchResult -= value;
        }

        public int TotalUnseenMessages => controller.TotalUnseenMessages;

        public ChatChannelsControllerMock(
            ChatController controller,
            UserProfileController userProfileController)
        {
            this.controller = controller;
            controller.OnAddMessage += UpdateNearbyUnseenMessages;
            this.userProfileController = userProfileController;

            SimulateDelayedResponseFor_ChatInitialization().ContinueWith(SendNearbyDescriptionMessage).Forget();
            AddFakeMessagesFromInput(CancellationToken.None).Forget();
        }

        public List<ChatMessage> GetAllocatedEntries() => controller.GetAllocatedEntries();

        public List<ChatMessage> GetPrivateAllocatedEntriesByUser(string userId) =>
            controller.GetPrivateAllocatedEntriesByUser(userId);

        public void GetUnseenMessagesByChannel() => controller.GetUnseenMessagesByChannel();

        public void Send(ChatMessage message)
        {
            message.timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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
            var chatMessageToLower = chatMessage.ToLower();

            if (chatMessageToLower.StartsWith("/join "))
            {
                var channelId = chatMessageToLower.Split(' ')[1].Replace("#", "");

                if (!chatMessageToLower.Contains("error"))
                    JoinOrCreateChannel(channelId);
                else
                {
                    await UniTask.Delay(Random.Range(40, 1000));
                    controller.JoinChannelError(CreateMockedDataFor_JoinChannelErrorPayload(channelId));
                }
            }
        }

        public void MarkMessagesAsSeen(string userId) => controller.MarkMessagesAsSeen(userId);

        public void MarkChannelMessagesAsSeen(string channelId)
        {
            currentChannelId = channelId;
            controller.MarkChannelMessagesAsSeen(channelId);

            SimulateDelayedResponseFor_MarkChannelAsSeen(channelId).Forget();
        }

        public void GetPrivateMessages(string userId, int limit, string fromMessageId) =>
            controller.GetPrivateMessages(userId, limit, fromMessageId);

        public void JoinOrCreateChannel(string channelId)
        {
            currentChannelId = channelId;
            SimulateDelayedResponseFor_JoinOrCreateChannel(channelId)
                .ContinueWith(success =>
                {
                    if (success)
                        SendWelcomeMessage(channelId).Forget();
                })
                .Forget();
        }

        public void LeaveChannel(string channelId)
        {
            if (channelId == currentChannelId)
                currentChannelId = null;
            LeaveFakeChannel(channelId).Forget();
        }

        public void GetChannelMessages(string channelId, int limit, string fromMessageId)
        {
            currentChannelId = channelId;
        }

        public void GetJoinedChannels(int limit, int skip)
        {
            ChannelInfoPayloads fakeJoinedChannels = new ChannelInfoPayloads { channelInfoPayload = new ChannelInfoPayload[joinedChannels.Count] };

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

                fakeJoinedChannels.channelInfoPayload[i] = msg;
            }

            controller.UpdateChannelInfo(JsonUtility.ToJson(fakeJoinedChannels));
        }

        public void GetChannelsByName(int limit, string name, string paginationToken = null) =>
            SearchFakeChannels(limit, name).Forget();

        public void GetChannels(int limit, string paginationToken) =>
            SearchFakeChannels(limit, "").Forget();

        public void MuteChannel(string channelId) => MuteFakeChannel(channelId, true).Forget();
        
        public void UnmuteChannel(string channelId) => MuteFakeChannel(channelId, false).Forget();

        public Channel GetAllocatedChannel(string channelId) => controller.GetAllocatedChannel(channelId);

        public void GetUnseenMessagesByUser() => controller.GetUnseenMessagesByUser();

        public int GetAllocatedUnseenMessages(string userId) => controller.GetAllocatedUnseenMessages(userId);

        public int GetAllocatedUnseenChannelMessages(string channelId) =>
            controller.GetAllocatedUnseenChannelMessages(channelId);

        public void CreateChannel(string channelId)
        {
            currentChannelId = channelId;
            SimulateDelayedResponseFor_CreateChannel(channelId).Forget();
        }

        private async UniTask SimulateDelayedResponseFor_CreateChannel(string channelId)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            if (joinedChannels.Count >= CHANNEL_LIMIT)
            {
                controller.JoinChannelError(CreateMockedDataFor_JoinChannelErrorPayload(channelId));
                return;
            }

            if (!joinedChannels.Contains(channelId))
                joinedChannels.Add(channelId);

            ChannelInfoPayloads fakeCreatedChannels = new ChannelInfoPayloads { channelInfoPayload = new ChannelInfoPayload[1] };
            fakeCreatedChannels.channelInfoPayload[0] = new ChannelInfoPayload
            {
                description = "",
                channelId = channelId,
                joined = true,
                memberCount = 1,
                muted = false,
                unseenMessages = 0
            };

            controller.JoinChannelConfirmation(JsonUtility.ToJson(fakeCreatedChannels));
        }

        private async UniTask MuteFakeChannel(string channelId, bool muted)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            ChannelInfoPayloads fakeMutedChannels = new ChannelInfoPayloads { channelInfoPayload = new ChannelInfoPayload[1] };
            fakeMutedChannels.channelInfoPayload[0] = new ChannelInfoPayload
            {
                joined = true,
                channelId = channelId,
                muted = muted,
                memberCount = Random.Range(0, 16),
                unseenMessages = Random.Range(0, 16)
            };

            controller.UpdateChannelInfo(JsonUtility.ToJson(fakeMutedChannels));
        }

        private async UniTask SimulateDelayedResponseFor_ChatInitialization()
        {
            await UniTask.Delay(Random.Range(50, 1000));

            var payload = new InitializeChatPayload
            {
                totalUnseenMessages = 0
            };
            controller.InitializeChat(JsonUtility.ToJson(payload));
        }

        private async UniTask SimulateDelayedResponseFor_MarkChannelAsSeen(string channelId)
        {
            await UniTask.Delay(Random.Range(50, 1000));

            var currentChannelUnseenMessages = GetAllocatedUnseenChannelMessages(channelId);

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

            var totalPayload = new UpdateTotalUnseenMessagesPayload
            {
                total = Mathf.Max(0, TotalUnseenMessages - currentChannelUnseenMessages)
            };
            controller.UpdateTotalUnseenMessages(JsonUtility.ToJson(totalPayload));
        }
        
        private async UniTask AddFakeMessagesFromInput(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.NextFrame(cancellationToken);

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    FakeChannelMessage();
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    FakeCurrentChannelMessage();
                }
                
                if (Input.GetKeyDown(KeyCode.K))
                {
                    FakePrivateMessage();
                }
            }
        }

        private void FakeChannelMessage()
        {
            var joinedChannels = this.joinedChannels.Concat(new[] {"nearby"}).Distinct().ToList();
            var randomChannelId = joinedChannels[Random.Range(0, joinedChannels.Count)];

            var characters = new[]
            {
                'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', '0', '1', '2', '3', '4', '5', '6', '7', '8',
                '9'
            };

            var userId = "FakeUser-";
            for (var x = 0; x < 4; x++)
                userId += characters[Random.Range(0, characters.Length)];

            var profile = new UserProfileModel
            {
                userId = userId,
                name = userId,
                snapshots = new UserProfileModel.Snapshots {face256 = $"https://picsum.photos/seed/{userId}/256"}
            };

            userProfileController.AddUserProfileToCatalog(profile);

            var messagePayload =
                new ChatMessage(ChatMessage.Type.PUBLIC, userId, $"fake message {Random.Range(0, 16000)}")
                {
                    recipient = randomChannelId == "nearby" ? null : randomChannelId,
                    timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    isChannelMessage = true
                };

            controller.AddMessageToChatWindow(JsonUtility.ToJson(messagePayload));

            // skip nearby, the unseen messages are getting updated by triggering the add message event
            if (randomChannelId == "nearby") return;
            
            var totalUnseenMessagesByChannelPayload = new UpdateTotalUnseenMessagesByChannelPayload
            {
                unseenChannelMessages = new[]
                {
                    new UpdateTotalUnseenMessagesByChannelPayload.UnseenChannelMessage
                    {
                        channelId = randomChannelId,
                        count = GetAllocatedUnseenChannelMessages(randomChannelId) + 1
                    }
                }
            };
            controller.UpdateTotalUnseenMessagesByChannel(JsonUtility.ToJson(totalUnseenMessagesByChannelPayload));

            var totalUnseenMessagesPayload = new UpdateTotalUnseenMessagesPayload
            {
                total = TotalUnseenMessages + 1
            };
            controller.UpdateTotalUnseenMessages(JsonUtility.ToJson(totalUnseenMessagesPayload));
        }
        
        private void FakeCurrentChannelMessage()
        {
            var channelId = string.IsNullOrEmpty(currentChannelId)
                ? "nearby"
                : currentChannelId;

            var characters = new[]
            {
                'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', '0', '1', '2', '3', '4', '5', '6', '7', '8',
                '9'
            };

            var userId = "FakeUser-";
            for (var x = 0; x < 4; x++)
                userId += characters[Random.Range(0, characters.Length)];

            var profile = new UserProfileModel
            {
                userId = userId,
                name = userId,
                snapshots = new UserProfileModel.Snapshots {face256 = $"https://picsum.photos/seed/{userId}/256"}
            };

            userProfileController.AddUserProfileToCatalog(profile);

            var messagePayload =
                new ChatMessage(ChatMessage.Type.PUBLIC, userId, $"fake message {Random.Range(0, 16000)}")
                {
                    recipient = channelId == "nearby" ? null : channelId,
                    timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    isChannelMessage = true
                };

            controller.AddMessageToChatWindow(JsonUtility.ToJson(messagePayload));
            
            // skip nearby, the unseen messages are getting updated by triggering the add message event
            if (channelId == "nearby") return;

            var totalUnseenMessagesByChannelPayload = new UpdateTotalUnseenMessagesByChannelPayload
            {
                unseenChannelMessages = new[]
                {
                    new UpdateTotalUnseenMessagesByChannelPayload.UnseenChannelMessage
                    {
                        channelId = channelId,
                        count = GetAllocatedUnseenChannelMessages(channelId) + 1
                    }
                }
            };
            controller.UpdateTotalUnseenMessagesByChannel(JsonUtility.ToJson(totalUnseenMessagesByChannelPayload));

            var totalUnseenMessagesPayload = new UpdateTotalUnseenMessagesPayload
            {
                total = TotalUnseenMessages + 1
            };
            controller.UpdateTotalUnseenMessages(JsonUtility.ToJson(totalUnseenMessagesPayload));
        }
        
        private void FakePrivateMessage()
        {
            var characters = new[]
            {
                'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', '0', '1', '2', '3', '4', '5', '6', '7', '8',
                '9'
            };

            var userId = "FakeUser-";
            for (var x = 0; x < 4; x++)
                userId += characters[Random.Range(0, characters.Length)];
            
            var profile = new UserProfileModel
            {
                userId = userId,
                name = userId,
                snapshots = new UserProfileModel.Snapshots {face256 = $"https://picsum.photos/seed/{userId}/256"}
            };
            
            userProfileController.AddUserProfileToCatalog(profile);
            
            var msg = new ChatMessage
            {
                body = $"fake message {Random.Range(0, 16000)}",
                sender = userId,
                recipient = userProfileController.ownUserProfile.userId,
                messageType = ChatMessage.Type.PRIVATE,
                timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            controller.AddMessageToChatWindow(JsonUtility.ToJson(msg));
            
            var totalUnseenMessagesByChannelPayload = new UpdateTotalUnseenMessagesByUserPayload
            {
                unseenPrivateMessages = new[]
                {
                    new UpdateTotalUnseenMessagesByUserPayload.UnseenPrivateMessage
                    {
                        userId = userId,
                        count = GetAllocatedUnseenMessages(userId) + 1
                    }
                }
            };
            controller.UpdateTotalUnseenMessagesByUser(JsonUtility.ToJson(totalUnseenMessagesByChannelPayload));

            var totalUnseenMessagesPayload = new UpdateTotalUnseenMessagesPayload
            {
                total = TotalUnseenMessages + 1
            };
            controller.UpdateTotalUnseenMessages(JsonUtility.ToJson(totalUnseenMessagesPayload));
        }

        private async UniTask SendWelcomeMessage(string channelId)
        {
            await UniTask.Delay(500);
            
            var messagePayload =
                new ChatMessage(ChatMessage.Type.SYSTEM, "", @$"This is the start of the channel #{channelId}.\n
Invite others to join by quoting the channel name in other chats or include it as a part of your bio.")
                {
                    recipient = channelId,
                    timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    isChannelMessage = true
                };

            controller.AddMessageToChatWindow(JsonUtility.ToJson(messagePayload));
        }
        
        private string CreateMockedDataFor_ChannelInfoPayload(string channelId)
        {
            ChannelInfoPayloads fakeChannels = new ChannelInfoPayloads { channelInfoPayload = new ChannelInfoPayload[1] };
            fakeChannels.channelInfoPayload[0] = new ChannelInfoPayload
            {
                joined = true,
                channelId = channelId,
                muted = false,
                memberCount = Random.Range(0, 16),
                unseenMessages = Random.Range(0, 16),
                description = Random.Range(0, 2) == 0
                    ? ""
                    : "This is a test description for the channel. This will be used to describe the main purpose of the channel."
            };

            return JsonUtility.ToJson(fakeChannels);
        }

        private string CreateMockedDataFor_JoinChannelErrorPayload(string channelId)
        {
            var mockedPayload = new JoinChannelErrorPayload
            {
                channelId = channelId,
                errorCode = (int) ChannelErrorCode.LimitExceeded
            };

            return JsonUtility.ToJson(mockedPayload);
        }
        
        private async UniTask<bool> SimulateDelayedResponseFor_JoinOrCreateChannel(string channelId)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            if (joinedChannels.Count >= CHANNEL_LIMIT)
            {
                controller.JoinChannelError(CreateMockedDataFor_JoinChannelErrorPayload(channelId));
                return false;
            }
            
            if (!joinedChannels.Contains(channelId))
                joinedChannels.Add(channelId);

            controller.JoinChannelConfirmation(CreateMockedDataFor_ChannelInfoPayload(channelId));
            return true;
        }
        
        private async UniTask LeaveFakeChannel(string channelId)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            ChannelInfoPayloads fakeLeftChannels = new ChannelInfoPayloads { channelInfoPayload = new ChannelInfoPayload[1] };
            fakeLeftChannels.channelInfoPayload[0] = new ChannelInfoPayload
            {
                joined = false,
                channelId = channelId,
                muted = false,
                memberCount = Random.Range(0, 16),
                unseenMessages = 0
            };

            joinedChannels.Remove(channelId);
            controller.UpdateChannelInfo(JsonUtility.ToJson(fakeLeftChannels));
        }
        
        private async UniTask SearchFakeChannels(int limit, string name)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            var ids = new[]
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

            var channelPayloads = new ChannelInfoPayloads();
            List<ChannelInfoPayload> channelsInfo = new List<ChannelInfoPayload>();

            for (var i = 0; i < limit && i < ids.Length; i++)
            {
                var channelId = ids[i];
                if (!channelId.StartsWith(name) && !string.IsNullOrEmpty(name)) continue;

                channelsInfo.Add(new ChannelInfoPayload
                {
                    joined = joinedChannels.Contains(channelId),
                    channelId = channelId,
                    muted = false,
                    memberCount = Random.Range(0, 16),
                    unseenMessages = 0
                });
            }

            channelPayloads.channelInfoPayload = channelsInfo.ToArray();

            var payload = new ChannelSearchResultsPayload
            {
                channels = channelPayloads.channelInfoPayload,
                since = Guid.NewGuid().ToString()
            };
            
            controller.UpdateChannelSearchResults(JsonUtility.ToJson(payload));
        }
        
        private void SendNearbyDescriptionMessage()
        {
            var messagePayload =
                new ChatMessage(ChatMessage.Type.SYSTEM, "",
                    "Talk to the people around you. If you move far away from someone you will lose contact. All whispers will be displayed.")
                {
                    timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    isChannelMessage = true
                };

            controller.AddMessageToChatWindow(JsonUtility.ToJson(messagePayload));
        }
        
        private void UpdateNearbyUnseenMessages(ChatMessage message)
        {
            const string nearbyChannelId = "nearby";
            
            if (message.messageType == ChatMessage.Type.PUBLIC
                && string.IsNullOrEmpty(message.recipient)
                || message.recipient == nearbyChannelId)
            {
                var totalUnseenMessagesByChannelPayload = new UpdateTotalUnseenMessagesByChannelPayload
                {
                    unseenChannelMessages = new[]
                    {
                        new UpdateTotalUnseenMessagesByChannelPayload.UnseenChannelMessage
                        {
                            channelId = nearbyChannelId,
                            count = GetAllocatedUnseenChannelMessages(nearbyChannelId) + 1
                        }
                    }
                };
                controller.UpdateTotalUnseenMessagesByChannel(JsonUtility.ToJson(totalUnseenMessagesByChannelPayload));

                var totalUnseenMessagesPayload = new UpdateTotalUnseenMessagesPayload
                {
                    total = TotalUnseenMessages + 1
                };
                controller.UpdateTotalUnseenMessages(JsonUtility.ToJson(totalUnseenMessagesPayload));
            }
        }

        public void GetChannelInfo(string[] channelIds) =>
            GetFakeChannelInfo(channelIds).Forget();

        private async UniTask GetFakeChannelInfo(string[] channelIds)
        {
            await UniTask.Delay(Random.Range(40, 1000));

            ChannelInfoPayloads fakeChannels = new ChannelInfoPayloads { channelInfoPayload = new ChannelInfoPayload[channelIds.Length] };

            for (int i = 0; i < channelIds.Length; i++)
            {
                var msg = new ChannelInfoPayload
                {
                    joined = true,
                    channelId = channelIds[i],
                    muted = false,
                    memberCount = Random.Range(0, 100),
                    unseenMessages = 0
                };

                fakeChannels.channelInfoPayload[i] = msg;
            }

            controller.UpdateChannelInfo(JsonUtility.ToJson(fakeChannels));
        }

        public void GetChannelMembers(string channelId, int limit, int skip, string name) =>
            GetFakeChannelMembers(channelId, limit, skip, name).Forget();

        public void GetChannelMembers(string channelId, int limit, int skip) =>
            GetFakeChannelMembers(channelId, limit, skip, "").Forget();

        private async UniTask GetFakeChannelMembers(string channelId, int limit, int skip, string name)
        {
            List<string> members = new List<string>();

            for (int i = 0; i < 100; i++)
            {
                var profile = new UserProfileModel
                {
                    userId = $"{channelId.Substring(0, 3)}_member{i + 1}",
                    name = $"{channelId.Substring(0, 3)}_member{i + 1}",
                    snapshots = new UserProfileModel.Snapshots { face256 = $"https://picsum.photos/seed/{i}/256" }
                };

                userProfileController.AddUserProfileToCatalog(profile);
                members.Add(profile.userId);
            }

            await UniTask.Delay(Random.Range(40, 1000));

            List<ChannelMember> membersToUpdate = new List<ChannelMember>();
            for (var i = skip; i < skip + limit && i < members.Count; i++)
            {
                var userId = members[i];
                if (!userId.Contains(name) && !string.IsNullOrEmpty(name)) continue;

                membersToUpdate.Add(new ChannelMember
                {
                    userId = userId,
                    isOnline = Random.Range(0, 2) == 0
                });
            }

            var msg = new UpdateChannelMembersPayload
            {
                channelId = channelId,
                members = membersToUpdate.ToArray()
            };
            controller.UpdateChannelMembers(JsonUtility.ToJson(msg));
        }

        public Channel GetAllocatedChannelByName(string channelName) => null;
    }
}