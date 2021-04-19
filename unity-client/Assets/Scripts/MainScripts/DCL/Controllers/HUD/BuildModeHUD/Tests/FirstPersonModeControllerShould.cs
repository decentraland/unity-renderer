using NSubstitute;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDControllers
{
    public class FirstPersonModeControllerShould
    {
        private FirstPersonModeController firstPersonModeController;

        [SetUp]
        public void SetUp()
        {
            firstPersonModeController = new FirstPersonModeController();
            firstPersonModeController.Initialize(
                Substitute.For<IFirstPersonModeView>(),
                Substitute.For< ITooltipController>());
        }

        [TearDown]
        public void TearDown()
        {
            firstPersonModeController.Dispose();
        }

        [Test]
        public void ClickCorrectly()
        {
            // Arrange
            bool clicked = false;
            firstPersonModeController.OnClick += () => { clicked = true; };

            // Act
            firstPersonModeController.Click();

            // Assert
            Assert.IsTrue(clicked, "clicked is false!");
        }

        [Test]
        public void ShowTooltipCorrectly()
        {
            // Arrange
            BaseEventData testEventData = new BaseEventData(null);
            string testText = "Test text";

            // Act
            firstPersonModeController.ShowTooltip(testEventData, testText);

            // Assert
            firstPersonModeController.tooltipController.Received(1).ShowTooltip(testEventData);
            firstPersonModeController.tooltipController.Received(1).SetTooltipText(testText);
        }

        [Test]
        public void HideTooltipCorrectly()
        {
            // Act
            firstPersonModeController.HideTooltip();

            // Assert
            firstPersonModeController.tooltipController.Received(1).HideTooltip();
        }
    }
}
