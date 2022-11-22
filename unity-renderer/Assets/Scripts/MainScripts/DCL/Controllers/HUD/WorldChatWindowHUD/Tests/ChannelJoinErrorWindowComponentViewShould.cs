using NUnit.Framework;

namespace DCL.Chat.HUD
{
    public class ChannelJoinErrorWindowComponentViewShould
    {
        private ChannelJoinErrorWindowComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = ChannelJoinErrorWindowComponentView.Create();
        }

        [Test]
        public void Show()
        {
            view.Show("random");
            
            Assert.AreEqual($"There was an error while trying to join the channel #random. Please try again.", view.titleLabel.text);
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