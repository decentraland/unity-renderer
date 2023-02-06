using DCL.Social.Chat.Channels;
using NUnit.Framework;

namespace DCl.Social.Chat.Channels
{
    public class JoinChannelComponentViewShould
    {
        private JoinChannelComponentView view;

        [SetUp]
        public void Setup()
        {
            view = JoinChannelComponentView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void ConfigureRealmSelectorCorrectly()
        {
            // Arrange
            JoinChannelComponentModel testModel = new JoinChannelComponentModel
            {
                channelId = "TestId"
            };

            // Act
            view.Configure(testModel);

            // Assert
            Assert.AreEqual(testModel, view.model, "The model does not match after configuring the realm selector.");
        }

        [Test]
        public void SetChannelCorrectly()
        {
            // Arrange
            string testName = "TestName";

            // Act
            view.SetChannel(testName);

            // Assert
            Assert.AreEqual(testName, view.model.channelId, "The channel id does not match in the model.");
            Assert.AreEqual(view.titleText.text, string.Format(JoinChannelComponentView.MODAL_TITLE, testName));
        }

        [Test]
        public void ShowLoading()
        {
            view.ShowLoading();

            Assert.IsFalse(view.cancelButton.IsInteractable());
            Assert.IsFalse(view.confirmButton.IsInteractable());
            Assert.IsTrue(view.loadingSpinnerContainer.activeSelf);
        }

        [Test]
        public void HideLoading()
        {
            view.HideLoading();

            Assert.IsTrue(view.cancelButton.IsInteractable());
            Assert.IsTrue(view.confirmButton.IsInteractable());
            Assert.IsFalse(view.loadingSpinnerContainer.activeSelf);
        }
    }
}
