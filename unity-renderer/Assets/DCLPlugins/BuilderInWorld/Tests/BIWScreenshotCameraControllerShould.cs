using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Camera;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Tests
{
    public class BIWScreenshotCameraControllerShould 
    {
        private ScreenshotCameraController screenshotCameraController;
        private GameObject mockedGameobject;
        
        [SetUp]
        public void SetUp()
        {
            mockedGameobject = new GameObject();
            screenshotCameraController = new ScreenshotCameraController();
            screenshotCameraController.Init(BIWTestUtils.CreateMockedContext());
            screenshotCameraController.freeCameraMovement = Substitute.For<IFreeCameraMovement>();
            screenshotCameraController.freeCameraMovement.Configure().gameObject.Returns(mockedGameobject);
        }

        [TearDown]
        public void Dispose()
        {
            screenshotCameraController.Dispose();
            GameObject.Destroy(mockedGameobject);
        }
        
        [Test]
        public void TakeScreenshot()
        {
            //Assert
            IScreenshotCameraController.OnSnapshotsReady onSucces = snapshot => Assert.IsNotNull(snapshot);
            
            //Act
            screenshotCameraController.TakeSceneScreenshot(onSucces);
        }
        
        [Test]
        public void TakeScreenshotWithParameters()
        {
            //Assert
            IScreenshotCameraController.OnSnapshotsReady onSucces = snapshot => Assert.IsNotNull(snapshot);
            
            //Act
            screenshotCameraController.TakeSceneScreenshot(Vector3.back, Vector3.back, onSucces);
        }
    }
}

