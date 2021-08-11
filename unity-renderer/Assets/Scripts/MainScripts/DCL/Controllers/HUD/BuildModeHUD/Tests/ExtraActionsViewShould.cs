using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class ExtraActionsViewShould
    {
        private ExtraActionsView extraActionsView;

        [SetUp]
        public void SetUp() { extraActionsView = ExtraActionsView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(extraActionsView.gameObject); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            extraActionsView.gameObject.SetActive(!isActive);

            // Act
            extraActionsView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, extraActionsView.gameObject.activeSelf, "The active property does not match!");
        }

        [Test]
        public void ClickOnControlsCorrectly()
        {
            // Arrange
            bool isClicked = false;
            extraActionsView.OnControlsClicked += () => isClicked = true;

            // Act
            extraActionsView.OnControlsClick(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void ClickOnHideUICorrectly()
        {
            // Arrange
            bool isClicked = false;
            extraActionsView.OnHideUIClicked += () => isClicked = true;

            // Act
            extraActionsView.OnHideUIClick(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void ClickOnTutorialCorrectly()
        {
            // Arrange
            bool isClicked = false;
            extraActionsView.OnTutorialClicked += () => isClicked = true;

            // Act
            extraActionsView.OnTutorialClick();

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void ClickOnResetCorrectly()
        {
            // Arrange
            bool isClicked = false;
            extraActionsView.OnResetClicked += () => isClicked = true;

            // Act
            extraActionsView.OnResetClick(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void ClickOnResetCameraCorrectly()
        {
            // Arrange
            bool isClicked = false;
            extraActionsView.OnResetCameraClicked += () => isClicked = true;

            // Act
            extraActionsView.OnResetCameraClick(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetResetButtonInteractableCorrectly(bool isInteractable)
        {
            // Arrange
            extraActionsView.resetBtn.interactable = !isInteractable;

            // Act
            extraActionsView.SetResetButtonInteractable(isInteractable);

            // Assert
            Assert.AreEqual(isInteractable, extraActionsView.resetBtn.interactable, "The interactable property does not match!");
        }
    }
}