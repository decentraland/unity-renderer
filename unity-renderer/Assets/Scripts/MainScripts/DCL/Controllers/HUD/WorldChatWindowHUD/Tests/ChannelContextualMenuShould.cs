using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChannelContextualMenuShould
    {
        private ChannelContextualMenu view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<ChannelContextualMenu>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/ChannelOptionsContextualMenu.prefab"));
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
