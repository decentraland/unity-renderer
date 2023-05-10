using NUnit.Framework;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChatChannelComponentViewShould
    {
        private ChatChannelComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = GameObject.Instantiate(Resources.Load<ChatChannelComponentView>("SocialBarV1/ChatChannelHUD"));
            view.Setup(new PublicChatModel("channelId", "name", "desc", true, 5, false, true));
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void LeaveChannelWhenLeaveFromContextualMenu()
        {
            var called = false;
            view.OnLeaveChannel += () => called = true;

            view.optionsButton.onClick.Invoke();
            view.contextualMenu.leaveButton.onClick.Invoke();

            Assert.IsTrue(called);
        }
    }
}
