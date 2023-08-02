using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChatChannelComponentViewShould
    {
        private ChatChannelComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<ChatChannelComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/ChatChannelHUD.prefab"));

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
