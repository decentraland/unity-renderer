using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class BuildModeConfirmationModalViewShould
    {
        private BuildModeConfirmationModalView exitFromBiWModalView;

        [SetUp]
        public void SetUp() { exitFromBiWModalView = BuildModeConfirmationModalView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(exitFromBiWModalView.gameObject); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            exitFromBiWModalView.gameObject.SetActive(!isActive);

            // Act
            exitFromBiWModalView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, exitFromBiWModalView.gameObject.activeSelf, "Game object activate property does not match!");
        }

        [Test]
        public void SetTitleCorrectly()
        {
            // Arrange
            string testText = "Test text";
            exitFromBiWModalView.title.text = "";

            // Act
            exitFromBiWModalView.SetTitle(testText);

            // Assert
            Assert.AreEqual(testText, exitFromBiWModalView.title.text, "Title text does not match!");
        }

        [Test]
        public void SetSubTitleCorrectly()
        {
            // Arrange
            string testText = "Test text";
            exitFromBiWModalView.subTitle.text = "";

            // Act
            exitFromBiWModalView.SetSubTitle(testText);

            // Assert
            Assert.AreEqual(testText, exitFromBiWModalView.subTitle.text, "SubTitle text does not match!");
        }

        [Test]
        public void SetCancelButtonTextCorrectly()
        {
            // Arrange
            string testText = "Test text";
            exitFromBiWModalView.cancelButtonText.text = "";

            // Act
            exitFromBiWModalView.SetCancelButtonText(testText);

            // Assert
            Assert.AreEqual(testText, exitFromBiWModalView.cancelButtonText.text, "cancelButtonText text does not match!");
        }

        [Test]
        public void SetConfirmButtonTextCorrectly()
        {
            // Arrange
            string testText = "Test text";
            exitFromBiWModalView.confirmButtonText.text = "";

            // Act
            exitFromBiWModalView.SetConfirmButtonText(testText);

            // Assert
            Assert.AreEqual(testText, exitFromBiWModalView.confirmButtonText.text, "confirmButtonText text does not match!");
        }

        [Test]
        public void CancelExitCorrectly()
        {
            // Arrange
            bool canceled = false;
            exitFromBiWModalView.OnCancelExit += () => { canceled = true; };

            // Act
            exitFromBiWModalView.CancelExit();

            // Assert
            Assert.IsTrue(canceled, "The canceled flag is false!");
        }

        [Test]
        public void ConfirmExitCorrectly()
        {
            // Arrange
            bool confirmed = false;
            exitFromBiWModalView.OnConfirmExit += () => { confirmed = true; };

            // Act
            exitFromBiWModalView.ConfirmExit();

            // Assert
            Assert.IsTrue(confirmed, "The confirmed flag is false!");
        }
    }
}