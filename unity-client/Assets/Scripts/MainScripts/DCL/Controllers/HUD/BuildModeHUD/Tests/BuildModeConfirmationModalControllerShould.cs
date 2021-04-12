using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class BuildModeConfirmationModalControllerShould
    {
        private BuildModeConfirmationModalController exitFromBiWModalController;

        [SetUp]
        public void SetUp()
        {
            exitFromBiWModalController = new BuildModeConfirmationModalController();
            exitFromBiWModalController.Initialize(Substitute.For<IBuildModeConfirmationModalView>());
        }

        [TearDown]
        public void TearDown() { exitFromBiWModalController.Dispose(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Act
            exitFromBiWModalController.SetActive(isActive);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(isActive);
        }

        [Test]
        public void SetTitleCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            exitFromBiWModalController.SetTitle(testText);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetTitle(testText);
        }

        [Test]
        public void SetSubTitleCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            exitFromBiWModalController.SetSubTitle(testText);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetSubTitle(testText);
        }

        [Test]
        public void SetCancelButtonTextCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            exitFromBiWModalController.SetCancelButtonText(testText);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetCancelButtonText(testText);
        }

        [Test]
        public void SetConfirmButtonTextCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            exitFromBiWModalController.SetConfirmButtonText(testText);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetConfirmButtonText(testText);
        }

        [Test]
        public void CancelExitCorrectly()
        {
            // Arrange
            bool canceled = false;
            exitFromBiWModalController.OnCancelExit += () => { canceled = true; };

            // Act
            exitFromBiWModalController.CancelExit();

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(false);
            Assert.IsTrue(canceled, "The canceled flag is false!");
        }

        [Test]
        public void ConfirmExitCorrectly()
        {
            // Arrange
            bool confirmed = false;
            exitFromBiWModalController.OnConfirmExit += () => { confirmed = true; };

            // Act
            exitFromBiWModalController.ConfirmExit();

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(false);
            Assert.IsTrue(confirmed, "The confirmed flag is false!");
        }
    }
}