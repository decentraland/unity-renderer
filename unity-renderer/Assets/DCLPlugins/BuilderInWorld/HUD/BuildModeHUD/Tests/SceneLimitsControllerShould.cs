using DCL.Controllers;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class SceneLimitsControllerShould : IntegrationTestSuite_Legacy
    {
        private SceneLimitsController sceneLimitsController;

        [SetUp]
        public void SetUp()
        {
            sceneLimitsController = new SceneLimitsController();
            sceneLimitsController.Initialize(Substitute.For<ISceneLimitsView>());
        }

        [TearDown]
        public void TearDown() { sceneLimitsController.Dispose(); }

        [Test]
        public void SetParcelSceneCorrectly()
        {
            // Arrange
            IParcelScene testParcelScene = TestUtils.CreateTestScene();

            // Act
            sceneLimitsController.SetParcelScene((ParcelScene) testParcelScene);

            // Assert
            Assert.AreEqual(testParcelScene, sceneLimitsController.currentParcelScene, "");
        }

        [Test]
        public void EnableCorrectly()
        {
            // Act
            sceneLimitsController.Enable();

            // Assert
            sceneLimitsController.sceneLimitsView.Received(1).SetBodyActive(true);
            sceneLimitsController.sceneLimitsView.Received(1).SetDetailsToggleAsOpen();
        }

        [Test]
        public void DisableCorrectly()
        {
            // Act
            sceneLimitsController.Disable();

            // Assert
            sceneLimitsController.sceneLimitsView.Received(1).SetBodyActive(false);
            sceneLimitsController.sceneLimitsView.Received(1).SetDetailsToggleAsClose();
        }

        [Test]
        public void UpdateInfoCorrectly()
        {
            // Arrange
            sceneLimitsController.SetParcelScene(TestUtils.CreateTestScene());

            // Act
            sceneLimitsController.UpdateInfo();

            // Assert
            sceneLimitsController.sceneLimitsView.Received().SetLeftDescText(Arg.Any<string>());
            sceneLimitsController.sceneLimitsView.Received().SetRightDescText(Arg.Any<string>());
        }
    }
}