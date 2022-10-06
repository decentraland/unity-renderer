using System;
using DCL.Chat.Channels;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
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
            controller = new ChatNotificationController(new DataStore(),
                mainNotificationsView,
                topNotificationsView,
                chatController,
                userProfileBridge);
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
    }
}