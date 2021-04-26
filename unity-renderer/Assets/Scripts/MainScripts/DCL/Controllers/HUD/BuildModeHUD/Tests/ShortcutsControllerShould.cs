using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class ShortcutsControllerShould
    {
        private ShortcutsController shortcutsController;

        [SetUp]
        public void SetUp()
        {
            shortcutsController = new ShortcutsController();
            shortcutsController.Initialize(Substitute.For<IShortcutsView>());
        }

        [TearDown]
        public void TearDown()
        {
            shortcutsController.Dispose();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void EnableCorrectly(bool isActive)
        {
            // Act
            shortcutsController.SetActive(isActive);

            // Assert
            shortcutsController.publishPopupView.Received(1).SetActive(isActive);
        }

        [Test]
        public void ClickOnCloseButtonCorrectly()
        {
            // Arrange
            bool closeClicked = false;
            shortcutsController.OnCloseClick += () => { closeClicked = true; };

            // Act
            shortcutsController.CloseButtonClicked();

            // Assert
            Assert.IsTrue(closeClicked, "The closeClicked is false!");
        }
    }
}
