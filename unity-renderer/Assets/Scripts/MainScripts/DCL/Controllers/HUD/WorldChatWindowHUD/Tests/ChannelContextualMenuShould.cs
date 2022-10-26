using NUnit.Framework;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelContextualMenuShould
    {
        private ChannelContextualMenu view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(Resources.Load<ChannelContextualMenu>("SocialBarV1/ChannelOptionsContextualMenu"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(view.gameObject);
        }

        [Test]
        public void TriggerLeave()
        {
            var called = false;
            view.OnLeave += () => called = true;
            
            view.leaveButton.onClick.Invoke();
            
            Assert.IsTrue(called);
        }

        [TestCase("bleh")]
        [TestCase("woah")]
        public void SetTitle(string title)
        {
            view.SetHeaderTitle(title);
            
            Assert.AreEqual(title, view.headerTiler.text);
        }

        [Test]
        public void Hide()
        {
            view.closeButton.onClick.Invoke();
            
            Assert.IsFalse(view.gameObject.activeSelf);
        }
    }
}