using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowComponentViewShould
    {
        private ChannelLeaveErrorWindowComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<ChannelLeaveErrorWindowComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/ChannelLeaveErrorModal.prefab"));
        }

        [Test]
        public void Show()
        {
            view.Show("random");

            Assert.AreEqual("There was an error while trying to leave the channel #random. Please try again.", view.titleLabel.text);
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

        [Test]
        public void TriggerRetryEventWhenRetryButtonIsClicked()
        {
            var calls = 0;
            view.OnRetry += () => calls++;

            view.retryButton.onClick.Invoke();

            Assert.AreEqual(1, calls);
        }
    }
}
