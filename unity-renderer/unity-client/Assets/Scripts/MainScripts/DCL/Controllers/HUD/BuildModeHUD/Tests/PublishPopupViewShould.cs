using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class PublishPopupViewShould
    {
        private PublishPopupView publishPopupView;

        [SetUp]
        public void SetUp() { publishPopupView = PublishPopupView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(publishPopupView.gameObject); }

        [Test]
        public void PublishStartCorrectly()
        {
            // Arrange
            publishPopupView.gameObject.SetActive(false);
            publishPopupView.loadingBar.SetActive(false);
            publishPopupView.resultText.gameObject.SetActive(true);
            publishPopupView.closeButton.gameObject.SetActive(true);
            publishPopupView.errorDetailsText.gameObject.SetActive(true);
            publishPopupView.titleText.text = "";

            // Act
            publishPopupView.PublishStart();

            // Assert
            Assert.IsTrue(publishPopupView.gameObject.activeSelf, "game object activate property is false!");
            Assert.IsTrue(publishPopupView.loadingBar.gameObject.activeSelf, "loadingBar activate property is false!");
            Assert.IsFalse(publishPopupView.resultText.gameObject.activeInHierarchy, "resultText activate property is true!");
            Assert.IsFalse(publishPopupView.closeButton.gameObject.activeInHierarchy, "closeButton activate property is true!");
            Assert.IsFalse(publishPopupView.errorDetailsText.gameObject.activeInHierarchy, "errorDetailsText activate property is true!");
            Assert.AreEqual(PublishPopupView.TITLE_INITIAL_MESSAGE, publishPopupView.titleText.text, "titleText dies not march!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void PublishEndCorrectly(bool isOk)
        {
            // Arrange
            string testErrorMessage = "Test text";
            publishPopupView.loadingBar.SetActive(true);
            publishPopupView.titleText.text = "";
            publishPopupView.resultText.text = "";
            publishPopupView.resultText.gameObject.SetActive(false);
            publishPopupView.errorDetailsText.text = "test";
            publishPopupView.errorDetailsText.gameObject.SetActive(isOk);
            publishPopupView.closeButton.gameObject.SetActive(false);

            // Act
            publishPopupView.PublishEnd(isOk, testErrorMessage);

            // Assert
            if (isOk)
            {
                Assert.AreEqual(PublishPopupView.SUCCESS_TITLE_MESSAGE, publishPopupView.titleText.text, "titleText dies not march!");
                Assert.AreEqual(PublishPopupView.SUCCESS_MESSAGE, publishPopupView.resultText.text, "resultText dies not march!");
                Assert.AreEqual("", publishPopupView.errorDetailsText.text, "errorDetailsText dies not march!");
                Assert.IsFalse(publishPopupView.errorDetailsText.gameObject.activeInHierarchy, "errorDetailsText activate property is true!");
            }
            else
            {
                Assert.AreEqual(PublishPopupView.FAIL_TITLE_MESSAGE, publishPopupView.titleText.text, "titleText dies not march!");
                Assert.AreEqual(PublishPopupView.FAIL_MESSAGE, publishPopupView.resultText.text, "resultText dies not march!");
                Assert.AreEqual(testErrorMessage, publishPopupView.errorDetailsText.text, "errorDetailsText dies not march!");
                Assert.IsTrue(publishPopupView.errorDetailsText.gameObject.activeInHierarchy, "errorDetailsText activate property is false!");
            }
            Assert.IsFalse(publishPopupView.loadingBar.gameObject.activeSelf, "loadingBar activate property is false!");
            Assert.IsTrue(publishPopupView.resultText.gameObject.activeInHierarchy, "resultText activate property is false!");
            Assert.IsTrue(publishPopupView.closeButton.gameObject.activeInHierarchy, "closeButton activate property is false!");
        }
    }
}