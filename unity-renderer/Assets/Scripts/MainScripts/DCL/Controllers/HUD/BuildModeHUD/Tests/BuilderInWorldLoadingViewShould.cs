using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace Tests.BuildModeHUDViews
{
    public class BuilderInWorldLoadingViewShould
    {
        private BuilderInWorldLoadingView builderInWorldLoadingView;

        [SetUp]
        public void SetUp() { builderInWorldLoadingView = BuilderInWorldLoadingView.Create(); }

        [TearDown]
        public void TearDown()
        {
            builderInWorldLoadingView.Dispose();
            Object.Destroy(builderInWorldLoadingView.gameObject);
        }

        [Test]
        public void ShowCorrectly()
        {
            // Arrange
            builderInWorldLoadingView.gameObject.SetActive(false);
            builderInWorldLoadingView.currentTipIndex = -1;
            builderInWorldLoadingView.tipsCoroutine = null;

            // Act
            builderInWorldLoadingView.Show();

            // Assert
            Assert.IsTrue(builderInWorldLoadingView.gameObject.activeSelf, "The view activeSelf property is false!");
            if (builderInWorldLoadingView.loadingTips.Count > 0)
            {
                Assert.IsTrue(builderInWorldLoadingView.currentTipIndex >= 0, "currentTipIndex is less than 0!");
                Assert.NotNull(builderInWorldLoadingView.tipsCoroutine, "tipsCoroutine is null!");
            }
        }

        [Test]
        public void HideCorrectly()
        {
            // Arrange
            builderInWorldLoadingView.gameObject.SetActive(true);

            // Act
            builderInWorldLoadingView.Hide(true);

            // Assert
            Assert.IsFalse(builderInWorldLoadingView.gameObject.activeSelf, "The view activeSelf property is true!");
        }
    }
}