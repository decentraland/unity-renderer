using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace DCL.Chat.HUD
{
    public class ChatChannelComponentViewShould
    {
        private ChatChannelComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = ChatChannelComponentView.Create();
            view.Setup(new PublicChatModel("channelId", "name", "desc", 0, true, 5));
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