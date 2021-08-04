using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class PublicationDetailsViewShould
    {
        private PublicationDetailsView publicationDetailsView;

        [SetUp]
        public void SetUp() { publicationDetailsView = PublicationDetailsView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(publicationDetailsView.gameObject); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            publicationDetailsView.gameObject.SetActive(!isActive);

            // Act
            publicationDetailsView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, publicationDetailsView.gameObject.activeSelf, "The publicationDetailsView active property does not match!");
        }

        [Test]
        public void CancelCorrectly()
        {
            // Arrange
            bool isCancelClicked = false;
            publicationDetailsView.OnCancel += () => isCancelClicked = true;

            // Act
            publicationDetailsView.Cancel();

            // Assert
            Assert.IsTrue(isCancelClicked, "isCancelClicked is false!");
        }

        [Test]
        public void PublishCorrectly()
        {
            // Arrange
            string testName = "Test name";
            string testDesc = "Test des";
            publicationDetailsView.sceneNameInput.text = testName;
            publicationDetailsView.sceneDescriptionInput.text = testDesc;
            bool isPublishClicked = false;
            string sceneNameReceived = "";
            string sceneDescReceived = "";

            publicationDetailsView.OnPublish += (sceneName, sceneDesc) =>
            {
                isPublishClicked = true;
                sceneNameReceived = sceneName;
                sceneDescReceived = sceneDesc;
            };

            // Act
            publicationDetailsView.Publish();

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
            publicationDetailsView.sceneNameValidationText.enabled = !isActive;

            // Act
            publicationDetailsView.SetSceneNameValidationActive(isActive);

            // Assert
            Assert.AreEqual(isActive, publicationDetailsView.sceneNameValidationText.enabled, "sceneNameValidationText enable property does not match!");
        }

        [Test]
        public void SetSceneNameCorrectly()
        {
            // Arrange
            string testName = "Test name";

            // Act
            publicationDetailsView.SetSceneName(testName);

            // Assert
            Assert.AreEqual(testName, publicationDetailsView.sceneNameInput.text, "sceneNameInput text does not match!");
        }

        [Test]
        public void SetSceneDescriptionCorrectly()
        {
            // Arrange
            string testDesc = "Test desc";

            // Act
            publicationDetailsView.SetSceneDescription(testDesc);

            // Assert
            Assert.AreEqual(testDesc, publicationDetailsView.sceneDescriptionInput.text, "sceneDescriptionInput text does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPublishButtonActiveCorrectly(bool isActive)
        {
            // Arrange
            publicationDetailsView.publishButton.interactable = !isActive;

            // Act
            publicationDetailsView.SetPublishButtonActive(isActive);

            // Assert
            Assert.AreEqual(isActive, publicationDetailsView.publishButton.interactable, "publishButton interactable property does not match!");
        }

        [Test]
        public void GetSceneNameCorrectly()
        {
            // Arrange
            string testName = "Test name";
            publicationDetailsView.sceneNameInput.text = testName;

            // Act
            string currentName = publicationDetailsView.GetSceneName();

            // Assert
            Assert.AreEqual(testName, currentName, "The returned name does not match!");
        }

        [Test]
        public void GetSceneDescriptionCorrectly()
        {
            // Arrange
            string testDesc = "Test description";
            publicationDetailsView.sceneDescriptionInput.text = testDesc;

            // Act
            string currentDesc = publicationDetailsView.GetSceneDescription();

            // Assert
            Assert.AreEqual(testDesc, currentDesc, "The returned description does not match!");
        }

        [Test]
        public void GetSceneScreenshotTextureCorrectly()
        {
            // Arrange
            Texture2D testTexture = new Texture2D(512, 512);
            publicationDetailsView.sceneScreenshot.sprite = Sprite.Create(testTexture, new Rect(0.0f, 0.0f, testTexture.width, testTexture.height), new Vector2(0.5f, 0.5f));

            // Act
            Texture2D returnedTexture = publicationDetailsView.GetSceneScreenshotTexture();

            // Assert
            Assert.AreEqual(testTexture, returnedTexture, "The returned texture does not match!");
        }

        [Test]
        public void UpdateSceneNameCharCounterCorrectly()
        {
            // Arrange
            publicationDetailsView.sceneNameInput.text = "123456";
            publicationDetailsView.sceneNameCharCounterText.text = "";

            // Act
            publicationDetailsView.UpdateSceneNameCharCounter();

            // Assert
            Assert.AreEqual($"6/{publicationDetailsView.sceneNameCharLimit}", publicationDetailsView.sceneNameCharCounterText.text, "The scene name char counter have not been updated correctly!");
        }

        [Test]
        public void UpdateSceneDescriptionCharCounterCorrectly()
        {
            // Arrange
            publicationDetailsView.sceneDescriptionInput.text = "123456";
            publicationDetailsView.sceneDescriptionCharCounterText.text = "";

            // Act
            publicationDetailsView.UpdateSceneDescriptionCharCounter();

            // Assert
            Assert.AreEqual($"6/{publicationDetailsView.sceneDescriptionCharLimit}", publicationDetailsView.sceneDescriptionCharCounterText.text, "The scene description char counter have not been updated correctly!");
        }
    }
}