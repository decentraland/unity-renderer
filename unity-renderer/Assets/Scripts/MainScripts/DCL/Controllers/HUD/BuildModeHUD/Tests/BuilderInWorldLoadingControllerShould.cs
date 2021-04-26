using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class BuilderInWorldLoadingControllerShould
    {
        private BuilderInWorldLoadingController builderInWorldLoadingController;

        [SetUp]
        public void SetUp()
        {
            builderInWorldLoadingController = new BuilderInWorldLoadingController();
            builderInWorldLoadingController.Initialize(Substitute.For<IBuilderInWorldLoadingView>());
        }

        [TearDown]
        public void TearDown() { builderInWorldLoadingController.Dispose(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShowCorrectly(bool showTips)
        {
            // Act
            builderInWorldLoadingController.Show(showTips);

            // Assert
            builderInWorldLoadingController.initialLoadingView.Received(1).Show(showTips);
        }

        [Test]
        public void HideCorrectly()
        {
            // Act
            builderInWorldLoadingController.Hide();

            // Assert
            builderInWorldLoadingController.initialLoadingView.Received(1).Hide();
        }

        [Test]
        public void CancelLoadingCorrectly()
        {
            // Arrange
            bool loadingCanceled = false;
            builderInWorldLoadingController.OnCancelLoading += () => { loadingCanceled = true; };

            // Act
            builderInWorldLoadingController.CancelLoading();

            // Assert
            Assert.IsTrue(loadingCanceled, "loadingCanceled is false!");
        }
    }
}