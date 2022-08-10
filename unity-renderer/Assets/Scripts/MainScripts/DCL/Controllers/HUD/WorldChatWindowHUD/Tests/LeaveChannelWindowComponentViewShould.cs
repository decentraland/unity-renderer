using NUnit.Framework;

namespace DCL.Chat.HUD
{
    public class LeaveChannelWindowComponentViewShould
    {
        private LeaveChannelWindowComponentView leaveChannelComponentView;

        [SetUp]
        public void Setup()
        {
            leaveChannelComponentView = LeaveChannelWindowComponentView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            leaveChannelComponentView.Dispose();
        }

        [Test]
        public void ConfigureRealmSelectorCorrectly()
        {
            // Arrange
            LeaveChannelWindowComponentModel testModel = new LeaveChannelWindowComponentModel
            {
                channelId = "TestId"
            };

            // Act
            leaveChannelComponentView.Configure(testModel);

            // Assert
            Assert.AreEqual(testModel, leaveChannelComponentView.model, "The model does not match after configuring the realm selector.");
        }

        [Test]
        public void SetChannelCorrectly()
        {
            // Arrange
            string testName = "TestName";

            // Act
            leaveChannelComponentView.SetChannel(testName);

            // Assert
            Assert.AreEqual(testName, leaveChannelComponentView.model.channelId, "The channel id does not match in the model.");
        }
    }
}