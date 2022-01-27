using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Camera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Tests
{
    public class BIWScreenshotCameraControllerShould 
    {
        private ScreenshotCameraController screenshotCameraController;

        [SetUp]
        public void SetUp()
        {
            screenshotCameraController = new ScreenshotCameraController();
            screenshotCameraController.Init(BIWTestUtils.CreateMockedContext());
        }

        [TearDown]
        public void Dispose()
        {
            screenshotCameraController.Dispose();
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

