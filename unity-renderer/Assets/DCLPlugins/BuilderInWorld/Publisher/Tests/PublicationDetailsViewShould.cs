using NUnit.Framework;
using UnityEngine;

namespace Tests.BIWPublisherTest
{
    public class PublicationDetailsViewShould
    {
        private LandPublisherView landPublisherView;

        [SetUp]
        public void SetUp() { landPublisherView = LandPublisherView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(landPublisherView.gameObject); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            landPublisherView.gameObject.SetActive(!isActive);

            // Act
            landPublisherView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, landPublisherView.gameObject.activeSelf, "The publicationDetailsView active property does not match!");
        }

        [Test]
        public void CancelCorrectly()
        {
            // Arrange
            bool isCancelClicked = false;
            landPublisherView.OnCancel += () => isCancelClicked = true;

            // Act
            landPublisherView.Cancel();

            // Assert
            Assert.IsTrue(isCancelClicked, "isCancelClicked is false!");
        }

        [Test]
        public void PublishCorrectly()
        {
            // Arrange
            bool isPublishClicked = false;

            landPublisherView.OnPublish += () =>
            {
                isPublishClicked = true;
            };

            // Act
            landPublisherView.Publish();

            // Assert
            Assert.IsTrue(isPublishClicked, "isCancelClicked is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPublishButtonActiveCorrectly(bool isActive)
        {
            // Arrange
            landPublisherView.publishButton.interactable = !isActive;

            // Act
            landPublisherView.SetPublishButtonActive(isActive);

            // Assert
            Assert.AreEqual(isActive, landPublisherView.publishButton.interactable, "publishButton interactable property does not match!");
        }

        [Test]
        public void GetSceneNameCorrectly()
        {
            // Arrange
            string testName = "Test name";
            landPublisherView.nameInputField.SetText(testName);

            // Act
            string currentName = landPublisherView.GetSceneName();

            // Assert
            Assert.AreEqual(testName, currentName, "The returned name does not match!");
        }

        [Test]
        public void GetSceneDescriptionCorrectly()
        {
            // Arrange
            string testDesc = "Test description";
            landPublisherView.descriptionInputField.SetText(testDesc);

            // Act
            string currentDesc = landPublisherView.GetSceneDescription();

            // Assert
            Assert.AreEqual(testDesc, currentDesc, "The returned description does not match!");
        }

        [Test]
        public void GetSceneScreenshotTextureCorrectly()
        {
            // Arrange
            Texture2D testTexture = new Texture2D(512, 512);
            landPublisherView.sceneScreenshot.sprite = Sprite.Create(testTexture, new Rect(0.0f, 0.0f, testTexture.width, testTexture.height), new Vector2(0.5f, 0.5f));

            // Act
            Texture2D returnedTexture = landPublisherView.GetSceneScreenshotTexture();

            // Assert
            Assert.AreEqual(testTexture, returnedTexture, "The returned texture does not match!");
        }
    }
}