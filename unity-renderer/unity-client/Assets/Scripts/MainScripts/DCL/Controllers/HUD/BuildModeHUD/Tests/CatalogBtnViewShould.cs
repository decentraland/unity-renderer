using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class CatalogBtnViewShould
    {
        private CatalogBtnView catalogBtnView;

        [SetUp]
        public void SetUp()
        {
            catalogBtnView = CatalogBtnView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(catalogBtnView.gameObject);
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            catalogBtnView.OnCatalogButtonClick += () => isClicked = true;

            // Act
            catalogBtnView.OnPointerClick(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            catalogBtnView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            catalogBtnView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            catalogBtnView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The tooltip text does not match!");
            Assert.AreEqual(catalogBtnView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            catalogBtnView.OnHideTooltip += () => isHidden = true;

            // Act
            catalogBtnView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }
    }
}
