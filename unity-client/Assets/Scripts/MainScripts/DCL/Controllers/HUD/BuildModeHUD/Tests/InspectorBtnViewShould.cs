using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class InspectorBtnViewShould
    {
        private InspectorBtnView inspectorBtnView;

        [SetUp]
        public void SetUp()
        {
            inspectorBtnView = InspectorBtnView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(inspectorBtnView.gameObject);
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            inspectorBtnView.OnInspectorButtonClick += () => isClicked = true;

            // Act
            inspectorBtnView.OnPointerClick(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            inspectorBtnView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            inspectorBtnView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            inspectorBtnView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The tooltip text does not match!");
            Assert.AreEqual(inspectorBtnView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            inspectorBtnView.OnHideTooltip += () => isHidden = true;

            // Act
            inspectorBtnView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }
    }
}
