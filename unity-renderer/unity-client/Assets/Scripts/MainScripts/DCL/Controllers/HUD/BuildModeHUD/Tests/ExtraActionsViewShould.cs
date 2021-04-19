using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class ExtraActionsViewShould
    {
        private ExtraActionsView extraActionsView;

        [SetUp]
        public void SetUp()
        {
            extraActionsView = ExtraActionsView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(extraActionsView.gameObject);
        }

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
    }
}
