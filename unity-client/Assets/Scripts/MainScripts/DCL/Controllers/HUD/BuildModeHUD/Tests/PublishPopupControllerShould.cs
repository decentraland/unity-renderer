using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class PublishPopupControllerShould
    {
        private PublishPopupController publishPopupController;

        [SetUp]
        public void SetUp()
        {
            publishPopupController = new PublishPopupController();
            publishPopupController.Initialize(Substitute.For<IPublishPopupView>());
        }

        [TearDown]
        public void TearDown()
        {
            publishPopupController.Dispose();
        }

        [Test]
        public void PublishStartCorrectly()
        {
            // Act
            publishPopupController.PublishStart();

            // Assert
            publishPopupController.publishPopupView.Received(1).PublishStart();
        }

        [Test]
        public void PublishEndCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            publishPopupController.PublishEnd(testText);

            // Assert
            publishPopupController.publishPopupView.Received(1).PublishEnd(testText);
        }
    }
}
