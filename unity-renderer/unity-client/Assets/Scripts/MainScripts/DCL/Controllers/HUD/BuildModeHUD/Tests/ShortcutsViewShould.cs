using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class ShortcutsViewShould
    {
        private ShortcutsView shortcutsView;

        [SetUp]
        public void SetUp()
        {
            shortcutsView = ShortcutsView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(shortcutsView.gameObject);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            shortcutsView.gameObject.SetActive(!isActive);

            // Act
            shortcutsView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, shortcutsView.gameObject.activeSelf, "The active property does not match!");
        }

        [Test]
        public void ClickOnCloseCorrectly()
        {
            // Arrange
            bool isClosed = false;
            shortcutsView.OnCloseButtonClick += () => isClosed = true;

            // Act
            shortcutsView.OnCloseClick();

            // Assert
            Assert.IsTrue(isClosed, "isClosed is false!");
        }
    }
}
