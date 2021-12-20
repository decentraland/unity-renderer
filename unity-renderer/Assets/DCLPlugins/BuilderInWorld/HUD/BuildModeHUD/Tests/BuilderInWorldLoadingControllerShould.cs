using NSubstitute;
using NUnit.Framework;
using UnityEngine;

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
        public void TearDown()
        {
            builderInWorldLoadingController.Dispose();
        }

        [Test]
        public void ShowCorrectly()
        {
            // Act
            builderInWorldLoadingController.Show();

            // Assert
            builderInWorldLoadingController.initialLoadingView.Received(1).Show();
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
        public void SetPercentageCorrectly()
        {
            // Arrange
            float testPercentage = 15.3f;

            // Act
            builderInWorldLoadingController.SetPercentage(testPercentage);

            // Assert
            builderInWorldLoadingController.initialLoadingView.Received(1).SetPercentage(testPercentage);
        }
    }
}