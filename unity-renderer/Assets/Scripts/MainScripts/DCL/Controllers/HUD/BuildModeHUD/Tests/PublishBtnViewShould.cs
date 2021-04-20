using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class PublishBtnViewShould
    {
        private PublishBtnView publishBtnView;

        [SetUp]
        public void SetUp()
        {
            publishBtnView = PublishBtnView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(publishBtnView.gameObject);
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            publishBtnView.OnPublishButtonClick += () => isClicked = true;

            // Act
            publishBtnView.OnPointerClick();

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            publishBtnView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            publishBtnView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            publishBtnView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The event data does not match!");
            Assert.AreEqual(publishBtnView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            publishBtnView.OnHideTooltip += () => isHidden = true;

            // Act
            publishBtnView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetInteractableCorrectly(bool isInteractable)
        {
            // Arrange
            publishBtnView.mainButton.interactable = !isInteractable;

            // Act
            publishBtnView.SetInteractable(isInteractable);

            // Assert
            Assert.AreEqual(isInteractable, publishBtnView.mainButton.interactable, "The interactable property does not match!");
        }
    }
}
