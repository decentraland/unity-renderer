using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class PublicChatWindowComponentViewShould
    {
        private PublicChatWindowComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<PublicChatWindowComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/NearbyChatChannelHUD.prefab"));
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
        public void Configure()
        {
            view.Configure(new PublicChatModel("nearby", "nearby", "any description", true, 0, false, true));

            Assert.AreEqual("~nearby", view.nameLabel.text);
            Assert.IsFalse(view.muteToggle.isOn);
        }

        [Test]
        public void ConfigureAsMuted()
        {
            view.Configure(new PublicChatModel("nearby", "nearby", "any description", true, 0, true, true));

            Assert.AreEqual("~nearby", view.nameLabel.text);
            Assert.IsTrue(view.muteToggle.isOn);
        }

        [Test]
        public void TriggerClose()
        {
            var called = false;
            view.OnClose += () => called = true;

            view.closeButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerBack()
        {
            var called = false;
            view.OnBack += () => called = true;

            view.backButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerFocusWhenWindowIsClicked()
        {
            var clicked = false;
            view.OnClickOverWindow += () => clicked = true;

            view.OnPointerDown(null);

            Assert.IsTrue(clicked);
        }

        [Test]
        public void TriggerFocusWhenWindowIsHovered()
        {
            var focused = false;
            view.OnFocused += f => focused = f;

            view.OnPointerEnter(null);

            Assert.IsTrue(focused);
        }

        [Test]
        public void TriggerUnfocusWhenPointerExits()
        {
            var focused = true;
            view.OnFocused += f => focused = f;

            view.OnPointerExit(null);

            Assert.IsFalse(focused);
        }

        [Test]
        public void UpdateMembersCountCorrectly()
        {
            var testMembersCount = 4;

            view.UpdateMembersCount(testMembersCount);

            Assert.AreEqual(testMembersCount.ToString(), view.memberCountLabel.text);
        }
    }
}
