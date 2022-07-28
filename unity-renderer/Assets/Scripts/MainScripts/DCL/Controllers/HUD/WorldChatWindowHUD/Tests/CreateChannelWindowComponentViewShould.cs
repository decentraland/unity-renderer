using NUnit.Framework;

namespace DCL.Chat.HUD
{
    public class CreateChannelWindowComponentViewShould
    {
        private CreateChannelWindowComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = CreateChannelWindowComponentView.Create();
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

        [TestCase("error")]
        [TestCase("other error")]
        public void ShowError(string message)
        {
            view.ShowError(message);
            
            Assert.IsFalse(view.channelExistsContainer.activeSelf);
            Assert.IsFalse(view.channelExistsWithJoinOptionContainer.activeSelf);
            Assert.IsTrue(view.genericErrorLabel.gameObject.activeSelf);
            Assert.AreEqual(message, view.genericErrorLabel.text);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShowChannelExistsError(bool withJoinOptionEnabled)
        {
            view.ShowChannelExistsError(withJoinOptionEnabled);
            
            Assert.AreEqual(!withJoinOptionEnabled, view.channelExistsContainer.activeSelf);
            Assert.AreEqual(withJoinOptionEnabled, view.channelExistsWithJoinOptionContainer.activeSelf);
            Assert.IsFalse(view.genericErrorLabel.gameObject.activeSelf);
        }

        [Test]
        public void ClearError()
        {
            view.ClearError();
            
            Assert.IsFalse(view.channelExistsContainer.activeSelf);
            Assert.IsFalse(view.channelExistsWithJoinOptionContainer.activeSelf);
            Assert.IsFalse(view.genericErrorLabel.gameObject.activeSelf);
        }

        [Test]
        public void DisableCreationButton()
        {
            view.DisableCreateButton();
            
            Assert.IsFalse(view.createButton.interactable);
        }
        
        [Test]
        public void EnableCreationButton()
        {
            view.EnableCreateButton();
            
            Assert.IsTrue(view.createButton.interactable);
        }

        [TestCase("foo", "3/20")]
        [TestCase("woah", "4/20")]
        [TestCase("toomanycharswithoverflow3122535623", "17/20")]
        public void UpdateTextLength(string text, string expectedLength)
        {
            view.channelNameInput.text = text;
            
            Assert.AreEqual(expectedLength, view.channelNameLengthLabel.text);
        }

        [Test]
        public void TriggerCloseEventWhenCloseButtonClicks()
        {
            var called = false;
            view.OnClose += () => called = true;
            
            view.closeButtons[0].onClick.Invoke();
            
            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerChannelNavigationWhenJoinButtonClicks()
        {
            var called = false;
            view.OnOpenChannel += () => called = true;
            
            view.joinButton.onClick.Invoke();
            
            Assert.IsTrue(called);
        }
        
        [Test]
        public void TriggerSubmitWhenCreateButtonClicks()
        {
            var called = false;
            view.OnCreateSubmit += () => called = true;
            
            view.EnableCreateButton();
            view.createButton.onClick.Invoke();
            
            Assert.IsTrue(called);
        }
    }
}