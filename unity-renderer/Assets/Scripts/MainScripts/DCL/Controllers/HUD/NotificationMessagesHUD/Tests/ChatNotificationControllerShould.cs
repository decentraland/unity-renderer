using System;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;
using Object = UnityEngine.Object;

namespace DCL.Chat.Notifications
{
    public class ChatNotificationControllerShould
    {
        private ChatNotificationController controller;
        private IChatController chatController;
        private IMainChatNotificationsComponentView mainNotificationsView;
        private ITopNotificationsComponentView topNotificationsView;
        private IUserProfileBridge userProfileBridge;
        private GameObject topPanelTransform;
        private IProfanityFilter profanityFilter;
        private DataStore dataStore;

        [SetUp]
        public void SetUp()
        {
            chatController = Substitute.For<IChatController>();
            mainNotificationsView = Substitute.For<IMainChatNotificationsComponentView>();
            topNotificationsView = Substitute.For<ITopNotificationsComponentView>();
            topPanelTransform = new GameObject("TopPanelTransform");
            topNotificationsView.GetPanelTransform().Returns(topPanelTransform.transform);
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel {userId = "ownUserId"});
            userProfileBridge.GetOwn().Returns(ownUserProfile);
            profanityFilter = Substitute.For<IProfanityFilter>();
            dataStore = new DataStore();
            dataStore.settings.profanityChatFilteringEnabled.Set(false);
            controller = new ChatNotificationController(dataStore,
                mainNotificationsView,
                topNotificationsView,
                chatController,
                userProfileBridge,
                profanityFilter);
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
            chatController.GetAllocatedChannel("mutedChannel").Returns(new Channel("mutedChannel", "mutedChannel",
                0, 3, true, true, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                ChatMessage.Type.PUBLIC, "sender", "hey") {recipient = "mutedChannel"});

            topNotificationsView.DidNotReceiveWithAnyArgs().AddNewChatNotification((PublicChannelMessageNotificationModel) default);
            mainNotificationsView.DidNotReceiveWithAnyArgs()
                .AddNewChatNotification((PublicChannelMessageNotificationModel) default);
        }

        [Test]
        public void AddPublicMessageToTheView()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots {face256 = "face256"}
            });
            userProfileBridge.Get("sender").Returns(senderUserProfile);
            chatController.GetAllocatedChannel("mutedChannel")
                .Returns(new Channel("mutedChannel", "random-channel", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                    ChatMessage.Type.PUBLIC, "sender", "hey", (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                {recipient = "mutedChannel"});

            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ChannelId == "mutedChannel" &&
                m.ChannelName == "random-channel"));
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ChannelId == "mutedChannel" &&
                m.ChannelName == "random-channel"));
        }

        [Test]
        public void AddPrivateMessageToTheView()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots {face256 = "face256"}
            });
            userProfileBridge.Get("sender").Returns(senderUserProfile);

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                    ChatMessage.Type.PRIVATE, "sender", "hey", (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                {recipient = "me"});

            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ProfilePicture == "face256"));
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ProfilePicture == "face256"));
        }

        [Test]
        public void AddNearbyMessageToTheView()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "sender",
                name = "imsender",
                snapshots = new UserProfileModel.Snapshots {face256 = "face256"}
            });
            userProfileBridge.Get("sender").Returns(senderUserProfile);
            chatController.GetAllocatedChannel("nearby")
                .Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                ChatMessage.Type.PUBLIC, "sender", "hey", (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ChannelId == "nearby" &&
                m.ChannelName == "nearby"));
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "imsender" && m.Body == "hey" && m.ChannelId == "nearby" &&
                m.ChannelName == "nearby"));
        }

        [Test]
        public void AddPublicMessageToTheViewWhenSenderHasNoProfile()
        {
            chatController.GetAllocatedChannel("mutedChannel")
                .Returns(new Channel("mutedChannel", "random-channel", 0, 0, true, false, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                    ChatMessage.Type.PUBLIC, "sender", "hey", (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                {recipient = "mutedChannel"});

            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "sender"));
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(m =>
                m.MessageId == "mid" && m.Username == "sender"));
        }

        [TestCase("shit", "****")]
        [TestCase("ass bitching wtf", "*** ****ing wtf")]
        public void FilterProfanityWordsOnPublicMessages(string body, string expectedBody)
        {
            dataStore.settings.profanityChatFilteringEnabled.Set(true);
            profanityFilter.Filter(body).Returns(UniTask.FromResult(expectedBody));
            GivenProfile("sender", "senderName");
            
            chatController.GetAllocatedChannel("channel")
                .Returns(new Channel("channel", "random-channel", 0, 0, true, false, ""));
            
            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                ChatMessage.Type.PUBLIC, "sender", body, (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(p => p.Body == expectedBody));
            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PublicChannelMessageNotificationModel>(p => p.Body == expectedBody));
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
            
            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                ChatMessage.Type.PRIVATE, "sender", body, (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(p => p.Body == body));
            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<PrivateChatMessageNotificationModel>(p => p.Body == body));
        }

        private void GivenProfile(string userId, string userName)
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = userId,
                name = userName,
                snapshots = new UserProfileModel.Snapshots {face256 = "face256"}
            });
            userProfileBridge.Get(userId).Returns(senderUserProfile);
        }
    }
}