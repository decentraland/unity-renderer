using NSubstitute;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDControllers
{
    public class CatalogBtnControllerShould
    {
        private CatalogBtnController catalogBtnController;

        [SetUp]
        public void SetUp()
        {
            catalogBtnController = new CatalogBtnController();
            catalogBtnController.Initialize(
                Substitute.For<ICatalogBtnView>(),
                Substitute.For<ITooltipController>());
        }

        [TearDown]
        public void TearDown()
        {
            catalogBtnController.Dispose();
        }

        [Test]
        public void ClickCorrectly()
        {
            // Arrange
            bool clicked = false;
            catalogBtnController.OnClick += () => { clicked = true; };

            // Act
            catalogBtnController.Click();

            // Assert
            Assert.IsTrue(clicked, "clicked is false!");
        }

        [Test]
        public void ShowTooltipCorrectly()
        {
            // Arrange
            BaseEventData eventData = new BaseEventData(null);
            string testText = "Test text";

            // Act
            catalogBtnController.ShowTooltip(eventData, testText);

            // Assert
            catalogBtnController.tooltipController.Received(1).ShowTooltip(eventData);
            catalogBtnController.tooltipController.Received(1).SetTooltipText(testText);
        }

        [Test]
        public void HideTooltipCorrectly()
        {
            // Act
            catalogBtnController.HideTooltip();

            // Assert
            catalogBtnController.tooltipController.Received(1).HideTooltip();
        }
    }
}
