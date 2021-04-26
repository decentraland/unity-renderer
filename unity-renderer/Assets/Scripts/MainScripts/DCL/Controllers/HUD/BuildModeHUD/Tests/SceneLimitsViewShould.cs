using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Tests.BuildModeHUDViews
{
    public class SceneLimitsViewShould
    {
        private SceneLimitsView sceneLimitsView;

        [SetUp]
        public void SetUp()
        {
            sceneLimitsView = SceneLimitsView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(sceneLimitsView.gameObject);
        }

        [Test]
        public void SetUpdateCallbackCorrectly()
        {
            // Arrange
            UnityAction testAction = () => {};

            // Act
            sceneLimitsView.SetUpdateCallback(testAction);

            // Assert
            Assert.AreEqual(sceneLimitsView.updateInfoAction, testAction, "The update action does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isToggleDone = false;
            sceneLimitsView.OnToggleSceneLimitsInfo += () => isToggleDone = true;

            // Act
            sceneLimitsView.ToggleSceneLimitsInfo(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isToggleDone, "isToggleDone is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetBodyActiveCorrectly(bool isActive)
        {
            // Arrange
            sceneLimitsView.sceneLimitsBodyGO.SetActive(!isActive);

            // Act
            sceneLimitsView.SetBodyActive(isActive);

            // Assert
            Assert.AreEqual(isActive, sceneLimitsView.sceneLimitsBodyGO.activeSelf, "The body active property does not match!");
        }

        [Test]
        public void SetDetailsToggleAsOpenCorrectly()
        {
            // Arrange
            sceneLimitsView.detailsToggleBtn.sprite = null;

            // Act
            sceneLimitsView.SetDetailsToggleAsOpen();

            // Assert
            Assert.AreEqual(sceneLimitsView.openMenuSprite, sceneLimitsView.detailsToggleBtn.sprite, "The details toggle button sprite does not match!");
        }

        [Test]
        public void SetDetailsToggleAsCloseCorrectly()
        {
            // Arrange
            sceneLimitsView.detailsToggleBtn.sprite = null;

            // Act
            sceneLimitsView.SetDetailsToggleAsClose();

            // Assert
            Assert.AreEqual(sceneLimitsView.closeMenuSprite, sceneLimitsView.detailsToggleBtn.sprite, "The details toggle button sprite does not match!");
        }

        [Test]
        public void SetTitleTextCorrectly()
        {
            // Arrange
            string newText = "Test text";
            sceneLimitsView.titleTxt.text = "";

            // Act
            sceneLimitsView.SetTitleText(newText);

            // Assert
            Assert.AreEqual(newText, sceneLimitsView.titleTxt.text, "The title text does not match!");
        }

        [Test]
        public void SetLeftDescTextCorrectly()
        {
            // Arrange
            string newText = "Test text";
            sceneLimitsView.leftDescTxt.text = "";

            // Act
            sceneLimitsView.SetLeftDescText(newText);

            // Assert
            Assert.AreEqual(newText, sceneLimitsView.leftDescTxt.text, "The left text does not match!");
        }

        [Test]
        public void SetRightDescTextCorrectly()
        {
            // Arrange
            string newText = "Test text";
            sceneLimitsView.rightDescTxt.text = "";

            // Act
            sceneLimitsView.SetRightDescText(newText);

            // Assert
            Assert.AreEqual(newText, sceneLimitsView.rightDescTxt.text, "The right text does not match!");
        }
    }
}
