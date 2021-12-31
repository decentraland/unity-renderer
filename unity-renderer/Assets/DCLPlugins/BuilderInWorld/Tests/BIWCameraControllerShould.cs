using System.Collections;
using System.Collections.Generic;
using DCL.Camera;
using DCL.Controllers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Builder
{
    public class BIWCameraControllerShould
    {
        private CameraController cameraController;
        private GameObject dummyGameObject;

        [SetUp]
        public void SetUp()
        {
            cameraController = new CameraController();
            cameraController.Initialize(BIWTestUtils.CreateMockedContext());
            cameraController.freeCameraController = Substitute.For<IFreeCameraMovement>();
            cameraController.screenshotCameraController = Substitute.For<IScreenshotCameraController>();
            dummyGameObject = new GameObject("CameraControllerBIW");
            cameraController.freeCameraController.Configure().gameObject.Returns(dummyGameObject);
        }

        [TearDown]
        public void TearDown()
        {
            cameraController.Dispose();
            GameObject.Destroy(dummyGameObject);
        }

        [Test]
        public void ActivateCameraCorrectly()
        {
            //Arrange
            var scene = Substitute.For<IParcelScene>();
            scene.Configure().GetSceneTransform().Returns(dummyGameObject.transform);
            
            //Act
            cameraController.ActivateCamera(scene);
            
            //Assert
            cameraController.freeCameraController.Received().SetPosition(Arg.Any<Vector3>());
            cameraController.freeCameraController.Received().LookAt(Arg.Any<Vector3>());
        }

        [Test]
        public void AskForScreenshotCorrectly()
        {
            //Act
            cameraController.TakeSceneScreenshot(
                (x) => { }
            );
            
            //Assert
            cameraController.screenshotCameraController.Received().TakeSceneScreenshot(Arg.Any<IScreenshotCameraController.OnSnapshotsReady>());
        }
        
        [Test]
        public void AskForAerialScreenshotCorrectly()
        {
            //Act
            cameraController.TakeSceneAerialScreenshot(Substitute.For<IParcelScene>(),
                (x) => { }
            );
            
            //Assert
            cameraController.screenshotCameraController.Received().TakeSceneAerialScreenshot(Arg.Any<IParcelScene>(),Arg.Any<IScreenshotCameraController.OnSnapshotsReady>());
        }
        
        [Test]
        public void AskForScreenshotWithParametersCorrectly()
        {
            //Act
            cameraController.TakeSceneScreenshot(Vector3.back, Vector3.down, 50,50,
                (x) => { }
            );
            
            //Assert
            cameraController.screenshotCameraController.Received().TakeSceneScreenshot(Arg.Any<Vector3>(),Arg.Any<Vector3>(),Arg.Any<int>(), Arg.Any<int>(),Arg.Any<IScreenshotCameraController.OnSnapshotsReady>());
        }
        
        [Test]
        public void AskedForScreenshotFromResetPositionCorrectly()
        {
            //Act
            cameraController.TakeSceneScreenshotFromResetPosition(
                (x) => { }
            );
            
            //Assert
            cameraController.screenshotCameraController.Received().TakeSceneScreenshot(Arg.Any<Vector3>(),Arg.Any<Vector3>(),Arg.Any<IScreenshotCameraController.OnSnapshotsReady>());
        }
    }

}
