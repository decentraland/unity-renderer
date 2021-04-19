using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class TooltipViewShould
    {
        private TooltipView tooltipView;

        [SetUp]
        public void SetUp()
        {
            tooltipView = TooltipView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(tooltipView.gameObject);
        }

        [Test]
        public void SetTooltipPositionProperly()
        {
            // Arrange
            Vector3 oldPosition = new Vector3(0, 0, 0);
            Vector3 newPosition = new Vector3(5, 2, 0);
            tooltipView.tooltipRT.position = oldPosition;

            // Act
            tooltipView.SetTooltipPosition(newPosition);

            // Assert
            Assert.AreEqual(newPosition, tooltipView.tooltipRT.position, "The tooltip position does not match!");
        }

        [Test]
        public void SetTextProperly()
        {
            // Arrange
            string oldText = "Old text";
            string newText = "New text";
            tooltipView.tooltipTxt.text = oldText;

            // Act
            tooltipView.SetText(newText);

            // Assert
            Assert.AreEqual(newText, tooltipView.tooltipTxt.text, "The tooltip text does not match!");
        }

        [Test]
        public void SetTooltipAlphaProperly()
        {
            // Arrange
            float oldAlpha = 0f;
            float newAlpha = 0.5f;
            tooltipView.tooltipCG.alpha = oldAlpha;

            // Act
            tooltipView.SetTooltipAlpha(newAlpha);

            // Assert
            Assert.AreEqual(newAlpha, tooltipView.tooltipCG.alpha, "The tooltip alpha does not match!");
        }
    }
}
