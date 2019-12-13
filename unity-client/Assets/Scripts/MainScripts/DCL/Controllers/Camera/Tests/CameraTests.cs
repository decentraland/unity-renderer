using Cinemachine;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace CameraController_Test
{
    public class CameraControllerShould : TestsBase
    {
        private CameraController cameraController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return InitScene(spawnCharController: true);
            cameraController = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/CameraController")).GetComponent<CameraController>();
        }

        [Test]
        public void ReactToCameraChangeAction()
        {
            var currentCamera = cameraController.currentMode;
            cameraController.cameraChangeAction.RaiseOnTriggered();

            Assert.AreNotEqual(currentCamera, cameraController.currentMode);
        }

        [Test]
        [TestCase(CameraController.CameraMode.FirstPerson)]
        [TestCase(CameraController.CameraMode.ThirdPerson)]
        public void LiveCameraIsOn(CameraController.CameraMode cameraMode)
        {
            var vCam = cameraController.cachedModeToVirtualCamera[cameraMode];
            cameraController.SetCameraMode(cameraMode);
            Assert.IsTrue(vCam.VirtualCameraGameObject.activeSelf);
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
            var currentVcam = (cameraController.cachedModeToVirtualCamera[cameraController.currentMode] as CinemachineVirtualCamera);
            var pov = currentVcam.GetCinemachineComponent<CinemachinePOV>();

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

            Assert.AreEqual(pov.m_HorizontalAxis.Value, rotationEuler.y);
            Assert.AreEqual(pov.m_VerticalAxis.Value, rotationEuler.x);
        }

        [UnityTest]
        [TestCase(0, 0, 0, ExpectedResult = null)] //ExpectedResult is needed because the method is IEnumerator. The workaround is needed because there's no "UnityTestCase" to use with IEnumerators
        [TestCase(1, 2, 3, ExpectedResult = null)]
        [TestCase(3, 3, 3, ExpectedResult = null)]
        public IEnumerator UpdateCameraPositionSO(float posX, float posY, float posZ)
        {
            var currentVcam = (cameraController.cachedModeToVirtualCamera[cameraController.currentMode] as CinemachineVirtualCamera);

            var toFollow = new GameObject("ToFollow");
            toFollow.transform.position = new Vector3(posX, posY, posZ);
            currentVcam.Follow = toFollow.transform;

            yield return null;

            Assert.AreEqual(currentVcam.State.FinalPosition, CommonScriptableObjects.cameraPosition.Get());
        }

        [UnityTest]
        [TestCase(0, 0, ExpectedResult = null)] //ExpectedResult is needed because the method is IEnumerator. The workaround is needed because there's no "UnityTestCase" to use with IEnumerators
        [TestCase(90, 90, ExpectedResult = null)]
        [TestCase(45, 45, ExpectedResult = null)]
        public IEnumerator UpdateCameraRotationSO(float horizontalAxis, float verticalAxis)
        {
            var currentVcam = (cameraController.cachedModeToVirtualCamera[cameraController.currentMode] as CinemachineVirtualCamera);
            var pov = currentVcam.GetCinemachineComponent<CinemachinePOV>();
            pov.m_HorizontalAxis.Value = horizontalAxis;
            pov.m_VerticalAxis.Value = verticalAxis;

            yield return null;

            Assert.AreEqual(currentVcam.State.FinalOrientation * Vector3.forward, CommonScriptableObjects.cameraForward.Get());
        }

        [UnityTest]
        public IEnumerator ActivateAndDeactivateWithKernelRenderingToggleEvents()
        {
            RenderingController.i.DeactivateRendering();
            Assert.IsFalse(cameraController.cameraTransform.gameObject.activeSelf);

            RenderingController.i.ActivateRendering();
            Assert.IsTrue(cameraController.cameraTransform.gameObject.activeSelf);
            yield break;
        }
    }
}
