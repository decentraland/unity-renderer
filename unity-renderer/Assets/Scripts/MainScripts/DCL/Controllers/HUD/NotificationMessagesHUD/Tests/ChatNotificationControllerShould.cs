using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.ProfanityFiltering;
using DCL.SettingsCommon;
using DCL.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;
using Channel = DCL.Chat.Channels.Channel;
using Object = UnityEngine.Object;

namespace DCL.Chat.Notifications
{
    public class ChatNotificationControllerShould
    {
        private const string OWN_USER_ID = "ownUserId";
        private const string OWN_USER_NAME = "ownName";

        private ChatNotificationController controller;
        private IChatController chatController;
        private IFriendsController friendsController;
        private IMainChatNotificationsComponentView mainNotificationsView;
        private ITopNotificationsComponentView topNotificationsView;
        private IUserProfileBridge userProfileBridge;
        private GameObject topPanelTransform;
        private IProfanityFilter profanityFilter;
        private DataStore dataStore;
        private ISettingsRepository<AudioSettings> audioSettings;

        [SetUp]
        public void SetUp()
        {
            chatController = Substitute.For<IChatController>();
            friendsController = Substitute.For<IFriendsController>();
            mainNotificationsView = Substitute.For<IMainChatNotificationsComponentView>();
            topNotificationsView = Substitute.For<ITopNotificationsComponentView>();
            topPanelTransform = new GameObject("TopPanelTransform");
            topNotificationsView.GetPanelTransform().Returns(topPanelTransform.transform);
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel { userId = OWN_USER_ID, name = OWN_USER_NAME });
            userProfileBridge.GetOwn().Returns(ownUserProfile);
            profanityFilter = Substitute.For<IProfanityFilter>();
            dataStore = new DataStore();
            dataStore.settings.profanityChatFilteringEnabled.Set(false);
            audioSettings = Substitute.For<ISettingsRepository<AudioSettings>>();

            audioSettings.Data.Returns(new AudioSettings
            {
                chatNotificationType = AudioSettings.ChatNotificationType.None,
            });

            controller = new ChatNotificationController(dataStore,
                mainNotificationsView,
                topNotificationsView,
                chatController,
                friendsController,
                userProfileBridge,
                profanityFilter,
                audioSettings);

            // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["new_friend_requests"] = true } });
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            Object.Destroy(topPanelTransform);
        }

        [Test]
        public void FilterNotificationWhenChannelIsMuted()
        {
            chatController.GetAllocatedChannel("mutedChannel")
                          .Returns(new Channel("mutedChannel", "mutedChannel",
                               0, 3, true, true, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                    ChatMessage.Type.PUBLIC, "sender", "hey") { recipient = "mutedChannel" }
            });

            topNotificationsView.DidNotReceiveWithAnyArgs()
                                .AddNewChatNotification((PublicChannelMessageNotificationModel)default);

            mainNotificationsView.DidNotReceiveWithAnyArgs()
                                 .AddNewChatNotification((PublicChannelMessageNotificationModel)default);
        }

        [Test]
        public void AddPublicMessageToTheView()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots { face256 = "face256" }
            });

            userProfileBridge.Get("sender").Returns(senderUserProfile);

            chatController.GetAllocatedChannel("mutedChannel")
                          .Returns(new Channel("mutedChannel", "random-channel", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                        ChatMessage.Type.PUBLIC, "sender", "hey",
                        (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    { recipient = "mutedChannel" }
            });

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                     m.MessageId == "mid" && m.Username == "imsender"
                                                          && m.Body == "hey" && m.ChannelId == "mutedChannel"
                                                          && m.ChannelName == "random-channel"
                                                          && !m.ImTheSender));

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                      m.MessageId == "mid" && m.Username == "imsender"
                                                           && m.Body == "hey" && m.ChannelId == "mutedChannel"
                                                           && m.ChannelName == "random-channel"
                                                           && !m.ImTheSender));
        }

        [Test]
        public void AddPrivateMessageToTheView()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots { face256 = "face256" }
            });

            userProfileBridge.Get("sender").Returns(senderUserProfile);

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                        ChatMessage.Type.PRIVATE, "sender", "hey",
                        (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    { recipient = "me" }
            });

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(m =>
                                     m.MessageId == "mid" && m.PeerUsername == "imsender"
                                                          && m.Body == "hey" && m.ProfilePicture == "face256"
                                                          && m.SenderUsername == "imsender"
                                                          && !m.ImTheSender));

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(m =>
                                      m.MessageId == "mid" && m.PeerUsername == "imsender"
                                                           && m.Body == "hey" && m.ProfilePicture == "face256"
                                                           && m.SenderUsername == "imsender"
                                                           && !m.ImTheSender));
        }

        [Test]
        public void AddPrivateMessageToTheViewWhenImSender()
        {
            var recipientUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            recipientUserProfile.UpdateData(new UserProfileModel
            {
                userId = "recipient",
                name = "recipientName",
                snapshots = new UserProfileModel.Snapshots { face256 = "face256" }
            });

            userProfileBridge.Get("recipient").Returns(recipientUserProfile);

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                        ChatMessage.Type.PRIVATE, OWN_USER_ID, "hey",
                        (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    { recipient = "recipient" }
            });

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(m =>
                                     m.MessageId == "mid" && m.PeerUsername == "recipientName"
                                                          && m.Body == "hey" && m.ProfilePicture == "face256"
                                                          && m.SenderUsername == OWN_USER_NAME
                                                          && m.ImTheSender));

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(m =>
                                      m.MessageId == "mid" && m.PeerUsername == "recipientName"
                                                           && m.Body == "hey" && m.ProfilePicture == "face256"
                                                           && m.SenderUsername == OWN_USER_NAME
                                                           && m.ImTheSender));
        }

        [Test]
        public void AddFriendRequestNotificationToTheView()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots { face256 = "face256" }
            });

            userProfileBridge.Get("sender").Returns(senderUserProfile);

            friendsController.OnFriendRequestReceived += Raise.Event<Action<FriendRequest>>(new FriendRequest(
                "test",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                "sender",
                OWN_USER_ID,
                "hey"));

            topNotificationsView.Received(1)
                                .AddNewFriendRequestNotification(Arg.Is<FriendRequestNotificationModel>(m =>
                                     m.UserId == "sender" && m.UserName == "imsender" && m.Header.Contains("Friend Request") && m.Message == "wants to be your friend."));

            mainNotificationsView.Received(1)
                                 .AddNewFriendRequestNotification(Arg.Is<FriendRequestNotificationModel>(m =>
                                      m.UserId == "sender" && m.UserName == "imsender" && m.Header.Contains("Friend Request") && m.Message == "wants to be your friend."));
        }

        [Test]
        public void AddNearbyMessageToTheView()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots { face256 = "face256" }
            });

            userProfileBridge.Get("sender").Returns(senderUserProfile);

            chatController.GetAllocatedChannel("nearby")
                          .Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                    ChatMessage.Type.PUBLIC, "sender", "hey", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            });

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                     m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ChannelId == "nearby" &&
                                     m.ChannelName == "nearby"
                                     && !m.ImTheSender));

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                      m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ChannelId == "nearby" &&
                                      m.ChannelName == "nearby"
                                      && !m.ImTheSender));
        }

        [Test]
        public void AddNearbyMessageWhenImTheSender()
        {
            chatController.GetAllocatedChannel("nearby")
                          .Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                    ChatMessage.Type.PUBLIC, OWN_USER_ID, "hey", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            });

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                     m.MessageId == "mid" && m.Username == OWN_USER_NAME
                                                          && m.Body == "hey"
                                                          && m.ChannelId == "nearby"
                                                          && m.ChannelName == "nearby"
                                                          && m.ImTheSender));

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                      m.MessageId == "mid" && m.Username == OWN_USER_NAME
                                                           && m.Body == "hey"
                                                           && m.ChannelId == "nearby"
                                                           && m.ChannelName == "nearby"
                                                           && m.ImTheSender));
        }

        [Test]
        public void AddPublicMessageToTheViewWhenSenderHasNoProfile()
        {
            chatController.GetAllocatedChannel("mutedChannel")
                          .Returns(new Channel("mutedChannel", "random-channel", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                        ChatMessage.Type.PUBLIC, "sender", "hey",
                        (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    { recipient = "mutedChannel" }
            });

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                     m.MessageId == "mid" && m.Username == "sender" && !m.ImTheSender));

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                      m.MessageId == "mid" && m.Username == "sender" && !m.ImTheSender));
        }

        [Test]
        public void AddPublicMessageToTheViewWhenImSender()
        {
            chatController.GetAllocatedChannel("mutedChannel")
                          .Returns(new Channel("mutedChannel", "random-channel", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                        ChatMessage.Type.PUBLIC, OWN_USER_ID, "hey",
                        (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    { recipient = "mutedChannel" }
            });

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                     m.MessageId == "mid" && m.Username == OWN_USER_NAME && m.ImTheSender));

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                      m.MessageId == "mid" && m.Username == OWN_USER_NAME
                                                           && m.ImTheSender));
        }

        [TestCase("shit", "****")]
        [TestCase("ass bitching wtf", "*** ****ing wtf")]
        public void FilterProfanityWordsOnPublicMessages(string body, string expectedBody)
        {
            dataStore.settings.profanityChatFilteringEnabled.Set(true);
            profanityFilter.Filter(body, Arg.Any<CancellationToken>()).Returns(UniTask.FromResult(expectedBody));
            GivenProfile("sender", "senderName");

            chatController.GetAllocatedChannel("channel")
                          .Returns(new Channel("channel", "random-channel", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                    ChatMessage.Type.PUBLIC, "sender", body, (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            });

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(p => p.Body == expectedBody));

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(p => p.Body == expectedBody));
        }

        [Test]
        public void DoNotFilterProfanityWordsOnPrivateMessages()
        {
            const string body = "shit";
            dataStore.settings.profanityChatFilteringEnabled.Set(true);
            profanityFilter.Filter(body).Returns(UniTask.FromResult(body));
            GivenProfile("sender", "senderName");

            chatController.GetAllocatedChannel("channel")
                          .Returns(new Channel("channel", "random-channel", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                    ChatMessage.Type.PRIVATE, "sender", body, (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            });

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(p => p.Body == body));

            topNotificationsView.Received(1)
                                .AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(p => p.Body == body));
        }

        [Test]
        public void AddFriendRequestNotificationWhenIsApproved()
        {
            GivenProfile("friendId", "friendName");
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["new_friend_requests"] = true } });

            friendsController.OnSentFriendRequestApproved += Raise.Event<Action<FriendRequest>>(
                new FriendRequest("friendRequestId", 0, "ownId", "friendId", "hey"));

            mainNotificationsView.Received(1)
                                 .AddNewFriendRequestNotification(Arg.Is<FriendRequestNotificationModel>(f =>
                                      f.UserId == "friendId"
                                      && f.UserName == "friendName"
                                      && f.Header == "Friend Request accepted"
                                      && f.Message == "and you are friends now!"
                                      && f.IsAccepted == true));

            topNotificationsView.Received(1)
                                .AddNewFriendRequestNotification(Arg.Is<FriendRequestNotificationModel>(f =>
                                     f.UserId == "friendId"
                                     && f.UserName == "friendName"
                                     && f.Header == "Friend Request accepted"
                                     && f.Message == "and you are friends now!"
                                     && f.IsAccepted == true));
        }

        [Test]
        public void AddFriendRequestNotificationWhenIsReceived()
        {
            GivenProfile("friendId", "friendName");
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["new_friend_requests"] = true } });

            friendsController.OnFriendRequestReceived += Raise.Event<Action<FriendRequest>>(
                new FriendRequest("friendRequestId", 100, "friendId", OWN_USER_ID, "hey!"));

            mainNotificationsView.Received(1)
                                 .AddNewFriendRequestNotification(Arg.Is<FriendRequestNotificationModel>(f =>
                                      f.UserId == "friendId"
                                      && f.UserName == "friendName"
                                      && f.Header == "Friend Request received"
                                      && f.Message == "wants to be your friend."
                                      && f.IsAccepted == false
                                      && f.FriendRequestId == "friendRequestId"));

            topNotificationsView.Received(1)
                                .AddNewFriendRequestNotification(Arg.Is<FriendRequestNotificationModel>(f =>
                                     f.UserId == "friendId"
                                     && f.UserName == "friendName"
                                     && f.Header == "Friend Request received"
                                     && f.Message == "wants to be your friend."
                                     && f.IsAccepted == false
                                     && f.FriendRequestId == "friendRequestId"));
        }

        [Test]
        public void OpenChatWhenClickOnAnApprovedFriendRequest()
        {
            friendsController.GetAllocatedFriendRequest("fr")
                             .Returns(new FriendRequest("fr", 100, "sender", "receiver", ""));

            friendsController.IsFriend("sender").Returns(true);
            mainNotificationsView.OnClickedFriendRequest += Raise.Event<IMainChatNotificationsComponentView.ClickedNotificationDelegate>("fr", "sender", true);

            Assert.AreEqual("sender", dataStore.HUDs.openChat.Get());
        }

        [Test]
        public void OpenFriendRequestWhenClickOnAnPendingFriendRequest()
        {
            friendsController.GetAllocatedFriendRequest("fr")
                             .Returns(new FriendRequest("fr", 100, "sender", "receiver", ""));

            friendsController.IsFriend("sender").Returns(false);
            mainNotificationsView.OnClickedFriendRequest += Raise.Event<IMainChatNotificationsComponentView.ClickedNotificationDelegate>("fr", "sender", false);

            Assert.AreEqual("fr", dataStore.HUDs.openReceivedFriendRequestDetail.Get());
        }

        [Test]
        public void DoNotShowNotificationPanelWhenTheNotificationQueueIsEmpty()
        {
            mainNotificationsView.GetNotificationsCount().Returns(0);

            mainNotificationsView.OnPanelFocus += Raise.Event<Action<bool>>(true);

            mainNotificationsView.DidNotReceive().ShowPanel();
        }

        [Test]
        public void ShowNotificationPanelWhenAnyNotificationIsAdded()
        {
            mainNotificationsView.GetNotificationsCount().Returns(1);

            mainNotificationsView.OnPanelFocus += Raise.Event<Action<bool>>(true);

            mainNotificationsView.Received().ShowPanel();
        }

        [TestCase(AudioSettings.ChatNotificationType.MentionsOnly, true)]
        [TestCase(AudioSettings.ChatNotificationType.All, true)]
        [TestCase(AudioSettings.ChatNotificationType.None, false)]
        public void PlayMentionSfxWhenSettingsAllowsIt(AudioSettings.ChatNotificationType chatSfxSetting, bool expectedSfx)
        {
            audioSettings.Data.Returns(new AudioSettings
            {
                chatNotificationType = chatSfxSetting,
            });

            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots { face256 = "face256" }
            });

            userProfileBridge.Get("sender").Returns(senderUserProfile);

            chatController.GetAllocatedChannel("nearby")
                          .Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("mid",
                    ChatMessage.Type.PUBLIC, "sender", $"mention for @{OWN_USER_NAME}", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            });

            mainNotificationsView.Received(1)
                                 .AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                                      m.IsOwnPlayerMentioned && m.ShouldPlayMentionSfx == expectedSfx));
        }

        private void GivenProfile(string userId, string userName)
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = userId,
                name = userName,
                snapshots = new UserProfileModel.Snapshots { face256 = "face256" }
            });

            userProfileBridge.Get(userId).Returns(senderUserProfile);
        }
    }
}
