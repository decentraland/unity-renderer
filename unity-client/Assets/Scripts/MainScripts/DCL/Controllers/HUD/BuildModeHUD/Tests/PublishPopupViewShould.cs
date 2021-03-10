using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class PublishPopupViewShould
    {
        private PublishPopupView publishPopupView;

        [SetUp]
        public void SetUp()
        {
            publishPopupView = PublishPopupView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(publishPopupView.gameObject);
        }

        [Test]
        public void PublishStartCorrectly()
        {
            // Arrange
            publishPopupView.gameObject.SetActive(false);
            publishPopupView.publishingGO.SetActive(false);
            publishPopupView.publishingFinishedGO.SetActive(true);

            // Act
            publishPopupView.PublishStart();

            // Assert
            Assert.IsTrue(publishPopupView.gameObject.activeSelf, "game object activate property is false!");
            Assert.IsTrue(publishPopupView.publishingGO.activeSelf, "publishingGO activate property is false!");
            Assert.IsFalse(publishPopupView.publishingFinishedGO.activeSelf, "publishingFinishedGO activate property is true!");
        }

        [Test]
        public void PublishEndCorrectly()
        {
            // Arrange
            string message = "Test message";
            publishPopupView.publishingGO.SetActive(true);
            publishPopupView.publishingFinishedGO.SetActive(false);
            publishPopupView.publishStatusTxt.text = "";

            // Act
            publishPopupView.PublishEnd(message);

            // Assert
            Assert.IsFalse(publishPopupView.publishingGO.activeSelf, "publishingGO activate property is false!");
            Assert.IsTrue(publishPopupView.publishingFinishedGO.activeSelf, "publishingFinishedGO activate property is false!");
            Assert.AreEqual(message, publishPopupView.publishStatusTxt.text, "Publish Status text does not match!");
        }
    }
}
