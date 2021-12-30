using NUnit.Framework;
using UnityEngine;

namespace Tests.PublisherTest
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
            string testName = "Test name";
            string testDesc = "Test des";
            landPublisherView.sceneNameInput.text = testName;
            landPublisherView.sceneDescriptionInput.text = testDesc;
            bool isPublishClicked = false;
            string sceneNameReceived = "";
            string sceneDescReceived = "";

            landPublisherView.OnPublish += () =>
            {
                isPublishClicked = true;
            };

            // Act
            landPublisherView.Publish();

            // Assert
            Assert.IsTrue(isPublishClicked, "isCancelClicked is false!");
            Assert.AreEqual(testName, sceneNameReceived, "The scene name does not match!");
            Assert.AreEqual(testDesc, sceneDescReceived, "The scene description does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetSceneNameValidationActiveCorrectly(bool isActive)
        {
            // Arrange
            landPublisherView.sceneNameValidationText.enabled = !isActive;

            // Act
            landPublisherView.SetSceneNameValidationActive(isActive);

            // Assert
            Assert.AreEqual(isActive, landPublisherView.sceneNameValidationText.enabled, "sceneNameValidationText enable property does not match!");
        }

        [Test]
        public void SetSceneNameCorrectly()
        {
            // Arrange
            string testName = "Test name";

            // Act
            landPublisherView.SetSceneName(testName);

            // Assert
            Assert.AreEqual(testName, landPublisherView.sceneNameInput.text, "sceneNameInput text does not match!");
        }

        [Test]
        public void SetSceneDescriptionCorrectly()
        {
            // Arrange
            string testDesc = "Test desc";

            // Act
            landPublisherView.SetSceneDescription(testDesc);

            // Assert
            Assert.AreEqual(testDesc, landPublisherView.sceneDescriptionInput.text, "sceneDescriptionInput text does not match!");
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
            landPublisherView.sceneNameInput.text = testName;

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
            landPublisherView.sceneDescriptionInput.text = testDesc;

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

        [Test]
        public void UpdateSceneNameCharCounterCorrectly()
        {
            // Arrange
            landPublisherView.sceneNameInput.text = "123456";
            landPublisherView.sceneNameCharCounterText.text = "";

            // Act
            landPublisherView.UpdateSceneNameCharCounter();

            // Assert
            Assert.AreEqual($"6/{landPublisherView.sceneNameCharLimit}", landPublisherView.sceneNameCharCounterText.text, "The scene name char counter have not been updated correctly!");
        }

        [Test]
        public void UpdateSceneDescriptionCharCounterCorrectly()
        {
            // Arrange
            landPublisherView.sceneDescriptionInput.text = "123456";
            landPublisherView.sceneDescriptionCharCounterText.text = "";

            // Act
            landPublisherView.UpdateSceneDescriptionCharCounter();

            // Assert
            Assert.AreEqual($"6/{landPublisherView.sceneDescriptionCharLimit}", landPublisherView.sceneDescriptionCharCounterText.text, "The scene description char counter have not been updated correctly!");
        }
    }
}