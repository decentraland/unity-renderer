using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class PublicationDetailsControllerShould
    {
        private PublicationDetailsController publicationDetailsController;

        [SetUp]
        public void SetUp()
        {
            publicationDetailsController = new PublicationDetailsController();
            publicationDetailsController.Initialize(Substitute.For<IPublicationDetailsView>());
        }

        [TearDown]
        public void TearDown() { publicationDetailsController.Dispose(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Act
            publicationDetailsController.SetActive(isActive);

            // Assert
            publicationDetailsController.publicationDetailsView.Received(1).SetActive(isActive);
        }

        [Test]
        public void CancelCorrectly()
        {
            // Arrange
            bool isCancelClicked = false;
            publicationDetailsController.OnCancel += () => isCancelClicked = true;

            // Act
            publicationDetailsController.Cancel();

            // Assert
            publicationDetailsController.publicationDetailsView.Received(1).SetActive(false);
            Assert.IsTrue(isCancelClicked, "isCancelClicked is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void PublishCorrectly(bool isValidated)
        {
            // Arrange
            string testName = "Test name";
            string testDesc = "Test desc";
            publicationDetailsController.isValidated = isValidated;

            bool isPublishClicked = false;
            publicationDetailsController.OnConfirm += () => isPublishClicked = true;

            // Act
            publicationDetailsController.Publish(testName, testDesc);

            // Assert
            if (isValidated)
            {
                publicationDetailsController.publicationDetailsView.Received(1).SetActive(false);
                Assert.IsTrue(isPublishClicked, "isPublishClicked is false!");
            }
        }

        [Test]
        [TestCase("")]
        [TestCase("Test name")]
        public void ValidatePublicationInfoCorrectly(string sceneName)
        {
            // Arrange
            publicationDetailsController.isValidated = sceneName.Length == 0;

            // Act
            publicationDetailsController.ValidatePublicationInfo(sceneName);

            // Assert
            Assert.AreEqual(sceneName.Length > 0, publicationDetailsController.isValidated, "isValidated does not match!");
            publicationDetailsController.publicationDetailsView.Received().SetSceneNameValidationActive(!publicationDetailsController.isValidated);
            publicationDetailsController.publicationDetailsView.Received().SetPublishButtonActive(publicationDetailsController.isValidated);
        }

        [Test]
        public void SetDefaultPublicationInfoCorrectly()
        {
            // Act
            publicationDetailsController.SetDefaultPublicationInfo();

            // Assert
            publicationDetailsController.publicationDetailsView.Received().SetSceneName(PublicationDetailsController.DEFAULT_SCENE_NAME);
            publicationDetailsController.publicationDetailsView.Received().SetSceneDescription(PublicationDetailsController.DEFAULT_SCENE_DESC);
        }

        [Test]
        public void SetCustomPublicationInfoCorrectly()
        {
            // Arrange
            string testName = "Test name";
            string testDesc = "Test desc";

            // Act
            publicationDetailsController.SetCustomPublicationInfo(testName, testDesc);

            // Assert
            publicationDetailsController.publicationDetailsView.Received().SetSceneName(testName);
            publicationDetailsController.publicationDetailsView.Received().SetSceneDescription(testDesc);
        }

        [Test]
        public void GetSceneNameCorrectly()
        {
            // Act
            publicationDetailsController.GetSceneName();

            // Assert
            publicationDetailsController.publicationDetailsView.Received().GetSceneName();
        }

        [Test]
        public void GetSceneDescriptionCorrectly()
        {
            // Act
            publicationDetailsController.GetSceneDescription();

            // Assert
            publicationDetailsController.publicationDetailsView.Received().GetSceneDescription();
        }

        [Test]
        public void GetSceneScreenshotTextureCorrectly()
        {
            // Act
            publicationDetailsController.GetSceneScreenshotTexture();

            // Assert
            publicationDetailsController.publicationDetailsView.Received().GetSceneScreenshotTexture();
        }
    }
}