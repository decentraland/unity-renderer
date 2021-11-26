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
            cameraController.freeCameraController.Received().TakeSceneScreenshot(Arg.Any<IFreeCameraMovement.OnSnapshotsReady>());
        }
        
        [Test]
        public void AskedForScreenshotFromResetPositionCorrectly()
        {
            //Act
            cameraController.TakeSceneScreenshotFromResetPosition(
                (x) => { }
            );

            //Assert
            cameraController.freeCameraController.Received().TakeSceneScreenshotFromResetPosition(Arg.Any<IFreeCameraMovement.OnSnapshotsReady>());
        }
    }

}
