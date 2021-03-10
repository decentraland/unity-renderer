using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDControllers
{
    public class TooltipControllerShould
    {
        private TooltipController tooltipController;

        [SetUp]
        public void SetUp()
        {
            tooltipController = new TooltipController();
            tooltipController.Initialize(Substitute.For<ITooltipView>());
        }

        [TearDown]
        public void TearDown()
        {
            tooltipController.Dispose();
        }

        [Test]
        public void SetTooltipTextCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            tooltipController.SetTooltipText(testText);

            // Assert
            tooltipController.view.Received(1).SetText(testText);
        }

        [Test]
        public void ShowTooltipCorrectly()
        {
            // Arrange
            PointerEventData testEventData = new PointerEventData(null);
            testEventData.pointerEnter = new GameObject("_PointerEnterGO");
            RectTransform testRT = testEventData.pointerEnter.AddComponent<RectTransform>();
            tooltipController.changeAlphaCoroutine = null;

            // Act
            tooltipController.ShowTooltip(testEventData);

            // Assert
            tooltipController.view.Received(1).SetTooltipPosition(testRT.position - Vector3.up * testRT.rect.height);
            Assert.IsNotNull(tooltipController.changeAlphaCoroutine, "The changeAlphaCoroutine is null!");
        }
    }
}
