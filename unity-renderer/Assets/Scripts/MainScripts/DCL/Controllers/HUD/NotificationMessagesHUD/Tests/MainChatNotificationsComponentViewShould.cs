using DCL.Interface;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public class MainChatNotificationsComponentViewShould : MonoBehaviour
    {
        private MainChatNotificationsComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<MainChatNotificationsComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/ChatNotificationHUD.prefab"));
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
                body, 0, "UsernameTest","UsernameTest", false, false));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = (ChatNotificationMessageComponentView)view.notificationQueue.Dequeue();

            Assert.AreEqual(body, addedNotification.notificationMessage.text);
            Assert.AreEqual("DM - UsernameTest", addedNotification.notificationHeader.text);
            Assert.AreEqual("UsernameTest:", addedNotification.notificationSender.text);
            Assert.IsTrue(addedNotification.imageContainer.activeSelf);
        }

        [Test]
        public void AddPrivateNotificationWhenImTheSender()
        {
            const string body = "This is a test message";

            view.AddNewChatNotification(new PrivateChatMessageNotificationModel("privateMessageId", "0x00000ba",
                body, 0, "UsernameTest","UsernameTest", true, false));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = (ChatNotificationMessageComponentView)view.notificationQueue.Dequeue();

            Assert.AreEqual(body, addedNotification.notificationMessage.text);
            Assert.AreEqual("DM - UsernameTest", addedNotification.notificationHeader.text);
            Assert.AreEqual("You:", addedNotification.notificationSender.text);
            Assert.IsFalse(addedNotification.imageContainer.activeSelf);
        }

        [Test]
        public void AddNearbyNotificationCorrectly()
        {
            const string body = "This is a test msg";
            ChatMessage newMessage = new ChatMessage(ChatMessage.Type.PUBLIC, "0x00000ba", body);
            view.AddNewChatNotification(new PublicChannelMessageNotificationModel("publicMessageId",
                body, "nearby", "nearby", 0, false, "UsernameTest", false, false));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = (ChatNotificationMessageComponentView)view.notificationQueue.Dequeue();

            Assert.AreEqual($"{newMessage.body}", addedNotification.model.message);
            Assert.AreEqual($"{newMessage.body}", addedNotification.notificationMessage.text);
            Assert.AreEqual("~nearby", addedNotification.model.messageHeader);
            Assert.AreEqual("~nearby", addedNotification.notificationHeader.text);
            Assert.AreEqual("nearby", addedNotification.model.notificationTargetId);
        }

        [Test]
        public void AddNearbyNotificationCorrectlyWhenImTheSender()
        {
            const string body = "This is a test msg";
            ChatMessage newMessage = new ChatMessage(ChatMessage.Type.PUBLIC, "0x00000ba", body);
            view.AddNewChatNotification(new PublicChannelMessageNotificationModel("publicMessageId",
                body, "nearby", "nearby", 0, true, "UsernameTest", false, false));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = (ChatNotificationMessageComponentView)view.notificationQueue.Dequeue();

            Assert.AreEqual($"{newMessage.body}", addedNotification.model.message);
            Assert.AreEqual($"{newMessage.body}", addedNotification.notificationMessage.text);
            Assert.AreEqual("~nearby", addedNotification.model.messageHeader);
            Assert.AreEqual("~nearby", addedNotification.notificationHeader.text);
            Assert.AreEqual("nearby", addedNotification.model.notificationTargetId);
            Assert.AreEqual("You:", addedNotification.model.messageSender);
        }

        [Test]
        public void AddCustomChannelNotificationCorrectly()
        {
            const string body = "This is a test msg";
            ChatMessage newMessage = new ChatMessage(ChatMessage.Type.PUBLIC, "0x00000ba", body);
            view.AddNewChatNotification(new PublicChannelMessageNotificationModel("publicMessageId",
                body, "my-channel", "oi34j5o24j52", 0, false, "UsernameTest", false, false));

            Assert.IsTrue(view.notificationQueue.Count == 1);
            Assert.IsTrue(view.poolableQueue.Count == 1);
            ChatNotificationMessageComponentView addedNotification = (ChatNotificationMessageComponentView)view.notificationQueue.Dequeue();

            Assert.AreEqual($"{newMessage.body}", addedNotification.model.message);
            Assert.AreEqual($"{newMessage.body}", addedNotification.notificationMessage.text);
            Assert.AreEqual("#my-channel", addedNotification.model.messageHeader);
            Assert.AreEqual("#my-channel", addedNotification.notificationHeader.text);
            Assert.AreEqual("oi34j5o24j52", addedNotification.model.notificationTargetId);
        }
    }
}
