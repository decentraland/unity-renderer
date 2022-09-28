using System;
using DCL.Chat.Channels;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Chat.HUD.NotificationMessages
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
            chatController.GetAllocatedChannel("mutedChannel").Returns(new Channel("mutedChannel", 0, 3, true,
                true, ""));

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                ChatMessage.Type.PUBLIC, "sender", "hey") {recipient = "mutedChannel"});

            topNotificationsView.DidNotReceiveWithAnyArgs().AddNewChatNotification(default);
            mainNotificationsView.DidNotReceiveWithAnyArgs().AddNewChatNotification(default);
        }

        [Test]
        public void AddMessageToTheView()
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
                ChatMessage.Type.PUBLIC, "sender", "hey", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) {recipient = "mutedChannel"});

            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<ChatMessage>(m => m.messageId == "mid"),
                "imsender", "face256");
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<ChatMessage>(m => m.messageId == "mid"),
                "imsender", "face256");
        }

        [Test]
        public void AddMessageToTheViewWhenSenderHasNoProfile()
        {
            chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage("mid",
                ChatMessage.Type.PUBLIC, "sender", "hey", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) {recipient = "mutedChannel"});

            topNotificationsView.Received(1).AddNewChatNotification(Arg.Is<ChatMessage>(m => m.messageId == "mid"),
                "sender");
            mainNotificationsView.Received(1).AddNewChatNotification(Arg.Is<ChatMessage>(m => m.messageId == "mid"),
                "sender");
        }
    }
}