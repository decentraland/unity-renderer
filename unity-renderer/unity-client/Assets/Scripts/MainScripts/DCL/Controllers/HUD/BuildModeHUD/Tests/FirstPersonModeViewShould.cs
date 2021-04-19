using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class FirstPersonModeViewShould
    {
        private FirstPersonModeView firstPersonModeView;

        [SetUp]
        public void SetUp()
        {
            firstPersonModeView = FirstPersonModeView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(firstPersonModeView.gameObject);
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            firstPersonModeView.OnFirstPersonModeClick += () => isClicked = true;

            // Act
            firstPersonModeView.OnPointerClick();

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            firstPersonModeView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            firstPersonModeView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            firstPersonModeView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The event data does not match!");
            Assert.AreEqual(firstPersonModeView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            firstPersonModeView.OnHideTooltip += () => isHidden = true;

            // Act
            firstPersonModeView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }
    }
}
