using NSubstitute;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDControllers
{
    public class TopActionsButtonsControllerShould
    {
        private TopActionsButtonsController topActionsButtonsController;

        [SetUp]
        public void SetUp()
        {
            topActionsButtonsController = new TopActionsButtonsController();
            topActionsButtonsController.Initialize(
                Substitute.For<ITopActionsButtonsView>(),
                Substitute.For<ITooltipController>(),
                Substitute.For<IBuildModeConfirmationModalController>());
        }

        [TearDown]
        public void TearDown() { topActionsButtonsController.Dispose(); }

        [Test]
        public void ClickOnChangeModeCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnChangeModeClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.ChangeModeClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void ClickOnExtraCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnExtraClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.ExtraClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void ClickOnTranslateCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnTranslateClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.TranslateClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void ClickOnRotateCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnRotateClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.RotateClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void ClickOnScaleCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnScaleClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.ScaleClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void ClickOnResetCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnResetClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.ResetClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void ClickOnDuplicateCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnDuplicateClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.DuplicateClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void ClickOnDeleteCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnDeleteClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.DeleteClicked();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void HideLogoutConfirmationCorrectly()
        {
            // Act
            topActionsButtonsController.HideLogoutConfirmation();

            // Assert
            topActionsButtonsController.buildModeConfirmationModalController.Received(1).SetActive(false);
        }

        [Test]
        public void ShowLogoutConfirmationCorrectly()
        {
            // Act
            topActionsButtonsController.ShowLogoutConfirmation();

            // Assert
            topActionsButtonsController.buildModeConfirmationModalController.Received(1).SetActive(true);
            topActionsButtonsController.buildModeConfirmationModalController.Received(1).SetTitle(Arg.Any<string>());
            topActionsButtonsController.buildModeConfirmationModalController.Received(1).SetSubTitle(Arg.Any<string>());
            topActionsButtonsController.buildModeConfirmationModalController.Received(1).SetCancelButtonText(Arg.Any<string>());
            topActionsButtonsController.buildModeConfirmationModalController.Received(1).SetConfirmButtonText(Arg.Any<string>());
        }

        [Test]
        public void ConfirmLogoutCorrectly()
        {
            // Arrange
            bool clicked = false;
            topActionsButtonsController.OnLogOutClick += () => { clicked = true; };

            // Act
            topActionsButtonsController.ConfirmLogout();

            // Assert
            Assert.IsTrue(clicked, "The clicked is false!");
        }

        [Test]
        public void TooltipPointerEnteredCorrectly()
        {
            // Arrange
            BaseEventData testEventData = new BaseEventData(null);
            string testText = "Test text";

            // Act
            topActionsButtonsController.TooltipPointerEntered(testEventData, testText);

            // Assert
            topActionsButtonsController.tooltipController.Received(1).ShowTooltip(testEventData);
            topActionsButtonsController.tooltipController.Received(1).SetTooltipText(testText);
        }

        [Test]
        public void TooltipPointerExitedCorrectly()
        {
            // Act
            topActionsButtonsController.TooltipPointerExited();

            // Assert
            topActionsButtonsController.tooltipController.Received(1).HideTooltip();
        }
    }
}