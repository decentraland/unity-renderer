using NSubstitute;
using NUnit.Framework;

namespace Tests.PublisherTest
{
    public class PublicationDetailsControllerShould
    {
        private LandPublisherController landPublisherController;

        [SetUp]
        public void SetUp()
        {
            landPublisherController = new LandPublisherController();
            landPublisherController.Initialize(Substitute.For<ILandPublisherView>());
        }

        [TearDown]
        public void TearDown() { landPublisherController.Dispose(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Act
            landPublisherController.SetActive(isActive);

            // Assert
            landPublisherController.landPublisherView.Received(1).SetActive(isActive);
        }

        [Test]
        public void CancelCorrectly()
        {
            // Arrange
            bool isCancelClicked = false;
            landPublisherController.OnCancel += () => isCancelClicked = true;

            // Act
            landPublisherController.Cancel();

            // Assert
            landPublisherController.landPublisherView.Received(1).SetActive(false);
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
            landPublisherController.isValidated = isValidated;

            bool isPublishClicked = false;
            //TODO: This will be reimplement when we have the definitive publish flow 
            
            // publicationDetailsController.OnConfirm += () => isPublishClicked = true;
            //
            // // Act
            // publicationDetailsController.Publish(testName, testDesc);

            // Assert
            if (isValidated)
            {
                landPublisherController.landPublisherView.Received(1).SetActive(false);
                Assert.IsTrue(isPublishClicked, "isPublishClicked is false!");
            }
        }

        [Test]
        [TestCase("")]
        [TestCase("Test name")]
        public void ValidatePublicationInfoCorrectly(string sceneName)
        {
            // Arrange
            landPublisherController.isValidated = sceneName.Length == 0;

            // Act
            landPublisherController.ValidatePublicationInfo(sceneName);

            // Assert
            Assert.AreEqual(sceneName.Length > 0, landPublisherController.isValidated, "isValidated does not match!");
            landPublisherController.landPublisherView.Received().SetSceneNameValidationActive(!landPublisherController.isValidated);
            landPublisherController.landPublisherView.Received().SetPublishButtonActive(landPublisherController.isValidated);
        }

        [Test]
        public void SetDefaultPublicationInfoCorrectly()
        {
            // Act
            landPublisherController.SetDefaultPublicationInfo();

            // Assert
            landPublisherController.landPublisherView.Received().SetSceneName(LandPublisherController.DEFAULT_SCENE_NAME);
            landPublisherController.landPublisherView.Received().SetSceneDescription(LandPublisherController.DEFAULT_SCENE_DESC);
        }

        [Test]
        public void SetCustomPublicationInfoCorrectly()
        {
            // Arrange
            string testName = "Test name";
            string testDesc = "Test desc";

            // Act
            //TODO: reeimplmentar
            // publicationDetailsController.SetCustomPublicationInfo(testName, testDesc);

            // Assert
            landPublisherController.landPublisherView.Received().SetSceneName(testName);
            landPublisherController.landPublisherView.Received().SetSceneDescription(testDesc);
        }

        [Test]
        public void GetSceneNameCorrectly()
        {
            //TODO: This will be reimplement when we have the definitive publish flow 
            // Act
            // publicationDetailsController.GetSceneName();

            // Assert
            landPublisherController.landPublisherView.Received().GetSceneName();
        }

        [Test]
        public void GetSceneDescriptionCorrectly()
        {
            // Act
            //TODO: This will be reimplement when we have the definitive publish flow 
            // publicationDetailsController.GetSceneDescription();

            // Assert
            landPublisherController.landPublisherView.Received().GetSceneDescription();
        }

        [Test]
        public void GetSceneScreenshotTextureCorrectly()
        {
            // Act
            //TODO: This will be reimplement when we have the definitive publish flow 
            // publicationDetailsController.GetSceneScreenshotTexture();

            // Assert
            landPublisherController.landPublisherView.Received().GetSceneScreenshotTexture();
        }
    }
}