using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace CameraController_Test
{
    public class CameraControllerShould : TestsBase
    {
        protected override bool enableSceneIntegrityChecker => false;

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
        [TestCase(1, 0, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(-1, 0, 0)]
        [TestCase(0, 0, -1)]
        [TestCase(-1, 0, -1)]
        public void ReactToSetRotation(float lookAtX, float lookAtY, float lookAtZ)
        {
            var payload = new CameraController.SetRotationPayload()
            {
                x = 0,
                y = 0,
                z = 0,
                cameraTarget = new Vector3(lookAtX, lookAtY, lookAtZ)
            };

            var rotationEuler = Quaternion.LookRotation(payload.cameraTarget.Value).eulerAngles;

            cameraController.SetRotation(JsonConvert.SerializeObject(payload, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            Assert.AreEqual(cameraController.GetRotation().y, rotationEuler.y);
            Assert.AreEqual(cameraController.GetRotation().x, rotationEuler.x);
        }

        [UnityTest]
        public IEnumerator ActivateAndDeactivateWithKernelRenderingToggleEvents()
        {
            RenderingController renderingController = GameObject.FindObjectOfType<RenderingController>();
            renderingController.DeactivateRendering();
            Assert.IsFalse(cameraController.camera.enabled);

            yield return null;

            renderingController.renderingActivatedAckLock.RemoveAllLocks();
            renderingController.ActivateRendering();
            Assert.IsTrue(cameraController.camera.enabled);
        }
    }
}