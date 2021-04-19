using NSubstitute;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDControllers
{
    public class PublishBtnControllerShould
    {
        private PublishBtnController publishBtnController;

        [SetUp]
        public void SetUp()
        {
            publishBtnController = new PublishBtnController();
            publishBtnController.Initialize(
                Substitute.For<IPublishBtnView>(),
                Substitute.For<ITooltipController>());
        }

        [TearDown]
        public void TearDown()
        {
            publishBtnController.Dispose();
        }

        [Test]
        public void ClickCorrectly()
        {
            // Arrange
            bool clicked = false;
            publishBtnController.OnClick += () => { clicked = true; };

            // Act
            publishBtnController.Click();

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
            publishBtnController.ShowTooltip(testEventData, testText);

            // Assert
            publishBtnController.tooltipController.Received(1).ShowTooltip(testEventData);
            publishBtnController.tooltipController.Received(1).SetTooltipText(testText);
        }

        [Test]
        public void HideTooltipCorrectly()
        {
            // Act
            publishBtnController.HideTooltip();

            // Assert
            publishBtnController.tooltipController.Received(1).HideTooltip();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetInteractableCorrectly(bool isInteractable)
        {
            // Act
            publishBtnController.SetInteractable(isInteractable);

            // Assert
            publishBtnController.publishBtnView.Received(1).SetInteractable(isInteractable);
        }
    }
}
