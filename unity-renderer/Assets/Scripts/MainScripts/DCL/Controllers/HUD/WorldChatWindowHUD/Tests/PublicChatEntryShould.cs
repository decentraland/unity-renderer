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
            view = Object.Instantiate(Resources.Load<PublicChatEntry>("SocialBarV1/ChannelSearchEntry"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(view.gameObject);
        }

        [TestCase(14)]
        [TestCase(8)]
        public void SetMemberCount(int members)
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", true, members, false, false));

            Assert.AreEqual($"{members} members joined", view.memberCountLabel.text);
        }

        [TestCase("bleh")]
        [TestCase("woo")]
        public void SetName(string name)
        {
            view.Configure(new PublicChatEntryModel("channelId", name, true, 0, false, false));
            
            Assert.AreEqual($"#{name}", view.nameLabel.text);
        }

        [Test]
        public void EnableJoinedContainerWhenIsJoined()
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", true, 0, false, false));
            
            Assert.IsTrue(view.joinedContainer.activeSelf);
        }
        
        [Test]
        public void DisableJoinedContainerWhenIsNotJoined()
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", false, 0, false, false));
            
            Assert.IsFalse(view.joinedContainer.activeSelf);
        }

        [Test]
        public void InitializeUnreadNotificationBadge()
        {
            view = PublicChatEntry.Create();
            var chatController = Substitute.For<IChatController>();
            chatController.GetAllocatedUnseenChannelMessages("channelId").Returns(5);
            view.Initialize(chatController);
            view.Configure(new PublicChatEntryModel("channelId", "bleh", true, 0, false, false));
            
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

        [Test]
        public void EnableLeaveOptionWhenIsJoined()
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", true, 0, false, false));
            
            Assert.IsTrue(view.joinedContainer.activeSelf);
        }
        
        [Test]
        public void DisableLeaveOptionWhenIsNotJoined()
        {
            view.Configure(new PublicChatEntryModel("channelId", "bleh", false, 0, false, false));
            
            Assert.IsFalse(view.joinedContainer.activeSelf);
        }

        [Test]
        public void TriggerLeave()
        {
            var called = false;
            view.OnLeave += entry => called = entry == view; 
            view.Configure(new PublicChatEntryModel("channelId", "bleh", true, 0, false, false));
            
            view.leaveButton.onClick.Invoke();
            
            Assert.IsTrue(called);
        }
    }
}