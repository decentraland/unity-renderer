using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class BuilderInWorldLoadingViewShould
    {
        private BuilderInWorldLoadingView builderInWorldLoadingView;

        [SetUp]
        public void SetUp() { builderInWorldLoadingView = BuilderInWorldLoadingView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(builderInWorldLoadingView.gameObject); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShowCorrectly(bool showTips)
        {
            // Arrange
            builderInWorldLoadingView.gameObject.SetActive(false);
            builderInWorldLoadingView.tipsText.text = string.Empty;

            // Act
            builderInWorldLoadingView.Show(showTips);

            // Assert
            Assert.IsTrue(builderInWorldLoadingView.gameObject.activeSelf, "The view activeSelf property is false!");
            if (showTips && builderInWorldLoadingView.loadingTips.Count > 0)
            {
                Assert.IsNotEmpty(builderInWorldLoadingView.tipsText.text, "tipsText is empty!");
                Assert.IsTrue(builderInWorldLoadingView.loadingTips.Contains(builderInWorldLoadingView.tipsText.text), "The set tipsText does not match!");
            }
            else
                Assert.IsEmpty(builderInWorldLoadingView.tipsText.text, "tipsText is not empty!");
        }

        [Test]
        public void HideCorrectly()
        {
            // Arrange
            builderInWorldLoadingView.gameObject.SetActive(true);

            // Act
            builderInWorldLoadingView.Hide();

            // Assert
            Assert.IsFalse(builderInWorldLoadingView.gameObject.activeSelf, "The view activeSelf property is true!");
        }

        [Test]
        public void CancelLoadingCorrectly()
        {
            // Arrange
            builderInWorldLoadingView.gameObject.SetActive(true);
            bool loadingCanceled = false;
            builderInWorldLoadingView.OnCancelLoading += () => { loadingCanceled = true; };

            // Act
            builderInWorldLoadingView.CancelLoading();

            // Assert
            Assert.IsFalse(builderInWorldLoadingView.gameObject.activeSelf, "The view activeSelf property is true!");
            Assert.IsTrue(loadingCanceled, "loadingCanceled is false!");
        }
    }
}