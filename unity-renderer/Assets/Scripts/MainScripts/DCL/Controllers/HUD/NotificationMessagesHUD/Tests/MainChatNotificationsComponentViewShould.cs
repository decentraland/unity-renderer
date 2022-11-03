using DCL.Interface;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public class MainChatNotificationsComponentViewShould : MonoBehaviour
    {
        private MainChatNotificationsComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = MainChatNotificationsComponentView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void Show()
        {
            view.Show();
            Assert.IsTrue(view.gameObject.activeSelf);
        }

        [Test]
        public void Hide()
        {
            view.Hide();
            Assert.IsFalse(view.gameObject.activeSelf);
        }

        [Test]
        public void CreateWithoutEntries()
        {
            Assert.IsTrue(view.notificationQueue.Count == 0);
            Assert.IsTrue(view.poolableQueue.Count == 0);
        }

        [Test]
        public void AddPrivateNotificationCorrectly()
        {
            const string body = "This is a test message";
        
            view.AddNewChatNotification(new PrivateChatMessageNotificationModel("privateMessageId", "0x00000ba",
                body, 0, "UsernameTest"));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = view.notificationQueue.Dequeue();

            Assert.AreEqual(body, addedNotification.model.message);
            Assert.AreEqual(body, addedNotification.notificationMessage.text);
            Assert.AreEqual("Private message", addedNotification.model.messageHeader);
            Assert.AreEqual("Private message", addedNotification.notificationHeader.text);
            Assert.AreEqual("0x00000ba", addedNotification.model.notificationTargetId);
        }

        [Test]
        public void AddNearbyNotificationCorrectly()
        {
            const string body = "This is a test msg";
            ChatMessage newMessage = new ChatMessage(ChatMessage.Type.PUBLIC, "0x00000ba", body);
            view.AddNewChatNotification(new PublicChannelMessageNotificationModel("publicMessageId",
                body, "nearby", "nearby", 0, "UsernameTest"));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = view.notificationQueue.Dequeue();

            Assert.AreEqual($"{newMessage.body}", addedNotification.model.message);
            Assert.AreEqual($"{newMessage.body}", addedNotification.notificationMessage.text);
            Assert.AreEqual("~nearby", addedNotification.model.messageHeader);
            Assert.AreEqual("~nearby", addedNotification.notificationHeader.text);
            Assert.AreEqual("nearby", addedNotification.model.notificationTargetId);
        }
    
        [Test]
        public void AddCustomChannelNotificationCorrectly()
        {
            const string body = "This is a test msg";
            ChatMessage newMessage = new ChatMessage(ChatMessage.Type.PUBLIC, "0x00000ba", body);
            view.AddNewChatNotification(new PublicChannelMessageNotificationModel("publicMessageId",
                body, "my-channel", "oi34j5o24j52", 0, "UsernameTest"));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = view.notificationQueue.Dequeue();

            Assert.AreEqual($"{newMessage.body}", addedNotification.model.message);
            Assert.AreEqual($"{newMessage.body}", addedNotification.notificationMessage.text);
            Assert.AreEqual("#my-channel", addedNotification.model.messageHeader);
            Assert.AreEqual("#my-channel", addedNotification.notificationHeader.text);
            Assert.AreEqual("oi34j5o24j52", addedNotification.model.notificationTargetId);
        }
    }
}