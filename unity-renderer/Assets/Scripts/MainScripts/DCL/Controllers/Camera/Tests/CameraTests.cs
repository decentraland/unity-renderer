using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Camera;
using DCL.CameraTool;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

namespace CameraController_Test
{
    public class CameraControllerShould : IntegrationTestSuite_Legacy
    {
        private CameraController cameraController => SceneReferences.i.cameraController;

        [Test]
        public void ReactToCameraChangeAction()
        {
            var currentCamera = CommonScriptableObjects.cameraMode.Get();
            cameraController.cameraChangeAction.RaiseOnTriggered();

            Assert.AreNotEqual(currentCamera, CommonScriptableObjects.cameraMode.Get());
        }

        [Test]
        [TestCase(CameraMode.ModeId.FirstPerson)]
        [TestCase(CameraMode.ModeId.ThirdPerson)]
        public void LiveCameraIsOn(CameraMode.ModeId cameraMode)
        {
            cameraController.SetCameraMode(cameraMode);
            Assert.IsTrue(cameraController.currentCameraState.defaultVirtualCamera.gameObject.activeInHierarchy);
        }

        [Test]
        [TestCase(1, -1, 0, ExpectedResult = new float[] { 45, 90 })]
        [TestCase(0, 1, 1, ExpectedResult = new float[] { -45, 0 })]
        [TestCase(1, 0, 1, ExpectedResult = new float[] { 0, 45 })]
        [TestCase(-1, 0, 0, ExpectedResult = new float[] { 0, -90 })]
        [TestCase(0, 0, -1, ExpectedResult = new float[] { 0, 180 })]
        [TestCase(-1, 0, -1, ExpectedResult = new float[] { 0, -135 })]
        public float[] ReactToSetRotation(float lookAtX, float lookAtY, float lookAtZ)
        {
            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.FirstPerson);
            var payload = new DCL.Camera.CameraController.SetRotationPayload()
            {
                x = 0,
                y = 0,
                z = 0,
                cameraTarget = new Vector3(lookAtX, lookAtY, lookAtZ)
            };

            cameraController.SetRotation(JsonConvert.SerializeObject(payload, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            return new[] { cameraController.GetRotation().x, cameraController.GetRotation().y };
        }

        [UnityTest]
        public IEnumerator ActivateAndDeactivateWithLoadingHUDToggleEvents()
        {
            CommonScriptableObjects.isLoadingHUDOpen.Set(true);
            yield return null;
            Assert.IsFalse(cameraController.camera.enabled);

            CommonScriptableObjects.isLoadingHUDOpen.Set(false);
            yield return null;
            Assert.IsTrue(cameraController.camera.enabled);
        }
        
        [UnityTest]
        public IEnumerator ActivateAndDeactivateWithFullscreenHUDToggleEvents()
        {
            CommonScriptableObjects.isFullscreenHUDOpen.Set(true);
            yield return null;
            Assert.IsFalse(cameraController.camera.enabled);

            CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
            yield return null;
            Assert.IsTrue(cameraController.camera.enabled);
        }
    }
}