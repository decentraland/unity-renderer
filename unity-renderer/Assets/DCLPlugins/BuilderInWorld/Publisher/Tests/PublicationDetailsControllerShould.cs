using NSubstitute;
using NUnit.Framework;

namespace Tests.BIWPublisherTest
{
    public class PublicationDetailsControllerShould
    {
        private LandPublisherController landPublisherController;

        [SetUp]
        public void SetUp()
        {
            landPublisherController = new LandPublisherController();
            landPublisherController.Initialize(Substitute.For<ILandPublisherView>());
        }

        [TearDown]
        public void TearDown() { landPublisherController.Dispose(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Act
            landPublisherController.SetActive(isActive);

            // Assert
            landPublisherController.landPublisherView.Received(1).SetActive(isActive);
        }

        [Test]
        public void CancelCorrectly()
        {
            // Arrange
            bool isCancelClicked = false;
            landPublisherController.OnPublishCancel += () => isCancelClicked = true;

            // Act
            landPublisherController.Cancel();

            // Assert
            landPublisherController.landPublisherView.Received(1).SetActive(false);
            Assert.IsTrue(isCancelClicked, "isCancelClicked is false!");
        }
    }
}