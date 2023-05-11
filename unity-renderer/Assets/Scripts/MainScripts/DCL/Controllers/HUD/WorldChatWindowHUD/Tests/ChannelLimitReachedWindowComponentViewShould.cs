using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelLimitReachedWindowComponentViewShould
    {
        private ChannelLimitReachedWindowComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<ChannelLimitReachedWindowComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/ChannelLimitReachedModal.prefab"));
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
        public void CloseWhenAcceptButtonClicks()
        {
            var calls = 0;
            view.OnClose += () => calls++;

            foreach (var button in view.acceptButton)
                button.onClick.Invoke();

            Assert.AreEqual(view.acceptButton.Length, calls);
        }
    }
}
