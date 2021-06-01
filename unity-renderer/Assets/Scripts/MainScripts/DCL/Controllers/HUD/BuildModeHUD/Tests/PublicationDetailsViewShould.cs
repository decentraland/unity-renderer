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
    }
}