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
        public void ConfigureCorrectly()
        {
            // Arrange
            string testTitleText = "Test title";
            string testSubTitleText = "Test subtitle";
            string testCancelBtnText = "Test cancel text";
            string testconfirmBtnText = "Test confirm text";

            // Act
            exitFromBiWModalController.Configure(testTitleText, testSubTitleText, testCancelBtnText, testconfirmBtnText);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetTitle(testTitleText);
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetSubTitle(testSubTitleText);
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetCancelButtonText(testCancelBtnText);
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetConfirmButtonText(testconfirmBtnText);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Act
            exitFromBiWModalController.SetActive(isActive, BuildModeModalType.EXIT);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(isActive);
        }

        [Test]
        public void CancelExitCorrectly()
        {
            // Arrange
            bool canceled = false;
            exitFromBiWModalController.OnCancelExit += (modalType) => { canceled = true; };

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
            exitFromBiWModalController.OnConfirmExit += (modalType) => { confirmed = true; };

            // Act
            exitFromBiWModalController.ConfirmExit();

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(false);
            Assert.IsTrue(confirmed, "The confirmed flag is false!");
        }
    }
}