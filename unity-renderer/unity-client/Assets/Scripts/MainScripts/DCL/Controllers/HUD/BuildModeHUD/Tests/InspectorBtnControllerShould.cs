using NSubstitute;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDControllers
{
    public class InspectorBtnControllerShould
    {
        private InspectorBtnController inspectorBtnController;

        [SetUp]
        public void SetUp()
        {
            inspectorBtnController = new InspectorBtnController();
            inspectorBtnController.Initialize(
                Substitute.For<IInspectorBtnView>(),
                Substitute.For<ITooltipController>());
        }

        [TearDown]
        public void TearDown()
        {
            inspectorBtnController.Dispose();
        }

        [Test]
        public void ClickCorrectly()
        {
            // Arrange
            bool clicked = false;
            inspectorBtnController.OnClick += () => { clicked = true; };

            // Act
            inspectorBtnController.Click();

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
            inspectorBtnController.ShowTooltip(testEventData, testText);

            // Assert
            inspectorBtnController.tooltipController.Received(1).ShowTooltip(testEventData);
            inspectorBtnController.tooltipController.Received(1).SetTooltipText(testText);
        }

        [Test]
        public void HideTooltipCorrectly()
        {
            // Act
            inspectorBtnController.HideTooltip();

            // Assert
            inspectorBtnController.tooltipController.Received(1).HideTooltip();
        }
    }
}
