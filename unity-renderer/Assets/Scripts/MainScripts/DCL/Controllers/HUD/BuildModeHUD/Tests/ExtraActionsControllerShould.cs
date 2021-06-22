using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class ExtraActionsControllerShould
    {
        private ExtraActionsController extraActionsController;

        [SetUp]
        public void SetUp()
        {
            extraActionsController = new ExtraActionsController();
            extraActionsController.Initialize(Substitute.For<IExtraActionsView>());
        }

        [TearDown]
        public void TearDown() { extraActionsController.Dispose(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Act
            extraActionsController.SetActive(isActive);

            // Assert
            extraActionsController.extraActionsView.Received(1).SetActive(isActive);
        }

        [Test]
        public void ClickOnControlsCorrectly()
        {
            // Arrange
            bool clicked = false;
            extraActionsController.OnControlsClick += () => { clicked = true; };

            // Act
            extraActionsController.ControlsClicked();

            // Assert
            Assert.IsTrue(clicked, "clicked is false!");
        }

        [Test]
        public void ClickOnHideUICorrectly()
        {
            // Arrange
            bool clicked = false;
            extraActionsController.OnHideUIClick += () => { clicked = true; };

            // Act
            extraActionsController.HideUIClicked();

            // Assert
            Assert.IsTrue(clicked, "clicked is false!");
        }

        [Test]
        public void ClickOnTutorialCorrectly()
        {
            // Arrange
            bool clicked = false;
            extraActionsController.OnTutorialClick += () => { clicked = true; };

            // Act
            extraActionsController.TutorialClicked();

            // Assert
            Assert.IsTrue(clicked, "clicked is false!");
        }

        [Test]
        public void ClickOnResetCameraCorrectly()
        {
            // Arrange
            bool clicked = false;
            extraActionsController.OnResetCameraClick += () => { clicked = true; };

            // Act
            extraActionsController.ResetCameraClicked();

            // Assert
            Assert.IsTrue(clicked, "clicked is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetResetButtonInteractableCorrectly(bool isInteractable)
        {
            // Act
            extraActionsController.SetResetButtonInteractable(isInteractable);

            // Assert
            extraActionsController.extraActionsView.Received(1).SetResetButtonInteractable(isInteractable);
        }
    }
}