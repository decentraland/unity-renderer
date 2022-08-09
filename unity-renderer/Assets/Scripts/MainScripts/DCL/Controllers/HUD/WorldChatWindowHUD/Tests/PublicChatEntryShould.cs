using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class PublicChatEntryShould
    {
        private PublicChatEntry view;

        [SetUp]
        public void SetUp()
        {
            view = Resources.Load<PublicChatEntry>("SocialBarV1/ChannelSearchEntry");
        }

        [TestCase(14)]
        [TestCase(8)]
        public void SetMemberCount(int members)
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", 0, true, members));

            Assert.AreEqual($"{members} members", view.memberCountLabel.text);
        }

        [TestCase("bleh")]
        [TestCase("woo")]
        public void SetName(string name)
        {
            view.Configure(new PublicChatEntryModel("channelId", name, 0, true, 0));
            
            Assert.AreEqual($"#{name}", view.nameLabel.text);
        }

        [Test]
        public void EnableJoinedContainerWhenIsJoined()
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", 0, true, 0));
            
            Assert.IsTrue(view.joinedContainer.activeSelf);
        }
        
        [Test]
        public void DisableJoinedContainerWhenIsNotJoined()
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", 0, false, 0));
            
            Assert.IsFalse(view.joinedContainer.activeSelf);
        }

        [Test]
        public void InitializeUnreadNotificationBadge()
        {
            view = PublicChatEntry.Create();
            var chatController = Substitute.For<IChatController>();
            chatController.GetAllocatedUnseenChannelMessages("channelId").Returns(5);
            view.Initialize(chatController);
            view.Configure(new PublicChatEntryModel("channelId", "bleh", 0, true, 0));
            
            Assert.AreEqual("5", view.unreadNotifications.notificationText.text);
        }

        [Test]
        public void TriggerOpenChat()
        {
            view = PublicChatEntry.Create();
            var called = false;
            view.OnOpenChat += entry => called = true; 
            view.openChatButton.onClick.Invoke();
            
            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerOptionsMenu()
        {
            view = PublicChatEntry.Create();
            view.GetComponent<UIHoverObjectToggler>().OnPointerEnter(null);
            var called = false;
            view.OnOpenOptions += entry => called = true;
            view.optionsButton.onClick.Invoke();
            
            Assert.IsTrue(called);
        }
    }
}