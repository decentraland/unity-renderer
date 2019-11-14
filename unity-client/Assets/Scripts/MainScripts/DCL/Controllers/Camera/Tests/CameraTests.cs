using System.Collections;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Camera_Test
{
    public class CameraControllerShould : TestsBase
    {
        private CameraController cameraController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return InitScene(spawnCharController: true);
            cameraController = Object.FindObjectOfType<CameraController>(); //Camera controller should be in the CharacterController
        }

        [Test]
        public void InitializeCameraSetupsCorrectly()
        {
            Assert.IsTrue(cameraController.cameraSetups.ContainsKey(CameraController.CameraState.FirstPerson));
            Assert.IsNotNull(cameraController.cameraSetups[CameraController.CameraState.FirstPerson]);
            Assert.IsTrue(cameraController.cameraSetups[CameraController.CameraState.FirstPerson] is FirstPersonCameraSetup);

            Assert.IsTrue(cameraController.cameraSetups.ContainsKey(CameraController.CameraState.ThirdPerson));
            Assert.IsNotNull(cameraController.cameraSetups[CameraController.CameraState.ThirdPerson]);
            Assert.IsTrue(cameraController.cameraSetups[CameraController.CameraState.ThirdPerson] is ThirdPersonCameraSetup);
        }

        [Test]
        public void ReactToCameraStateChanges()
        {
            var cameraStateSO = cameraController.currentState;

            cameraStateSO.Set(CameraController.CameraState.FirstPerson);
            Assert.IsTrue(cameraController.currentSetup is FirstPersonCameraSetup);

            cameraStateSO.Set(CameraController.CameraState.ThirdPerson);
            Assert.IsTrue(cameraController.currentSetup is ThirdPersonCameraSetup);
        }
    }

    public class CameraSetupFactoryShould : TestsBase
    {
        [Test]
        public void CreateFirstPersonSetupCorrectly()
        {
            var config = ScriptableObject.CreateInstance<FirstPersonCameraConfigSO>();
            var dummyTransform = new GameObject("_dummyTransform").transform;

            var cameraSetup = (FirstPersonCameraSetup)CameraSetupFactory.CreateCameraSetup(CameraController.CameraState.FirstPerson, dummyTransform, config);

            Assert.AreEqual(config, cameraSetup.configuration);
            Assert.AreEqual(dummyTransform, cameraSetup.cameraTransform);
        }
        
        [Test]
        public void CreateThirdPersonSetupCorrectly()
        {
            var config = ScriptableObject.CreateInstance<ThirdPersonCameraConfigSO>();
            var dummyTransform = new GameObject("_dummyTransform").transform;

            var cameraSetup = (ThirdPersonCameraSetup)CameraSetupFactory.CreateCameraSetup(CameraController.CameraState.ThirdPerson, dummyTransform, config);

            Assert.AreEqual(config, cameraSetup.configuration);
            Assert.AreEqual(dummyTransform, cameraSetup.cameraTransform);
        }
    }

    public class FirstPersonCameraShould : TestsBase
    {
        private FirstPersonCameraConfigSO config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return InitScene();
            config = ScriptableObject.CreateInstance<FirstPersonCameraConfigSO>();
        }

        [Test]
        public void NotModifyTheTransformWithoutActivation()
        {
            config.Set(new FirstPersonCameraConfig() { yOffset = 10 });
            var dummyTransform = new GameObject("_dummyTransform").transform;
            dummyTransform.position = Vector3.up * 1.5f;

            var cameraSetup = new FirstPersonCameraSetup(dummyTransform, config);

            Assert.AreEqual(Vector3.up * 1.5f, dummyTransform.position);
        }

        [Test]
        public void ModifyTheTransformOnActivation()
        {
            config.Set(new FirstPersonCameraConfig() { yOffset = 10 });
            var dummyTransform = new GameObject("_dummyTransform").transform;

            var cameraSetup = new FirstPersonCameraSetup(dummyTransform, config);
            cameraSetup.Activate();

            Assert.AreEqual(config.Get().yOffset, dummyTransform.position.y);
        }

        [Test]
        public void ReactToChangesInConfig()
        {
            config.Set(new FirstPersonCameraConfig() { yOffset = 10 });
            var dummyTransform = new GameObject("_dummyTransform").transform;
            var cameraSetup = new FirstPersonCameraSetup(dummyTransform, config);
            cameraSetup.Activate();

            config.Set(new FirstPersonCameraConfig() { yOffset = 77 });

            Assert.AreEqual(config.Get().yOffset, dummyTransform.position.y);
        }
    }

    public class ThirdPersonCameraShould : TestsBase
    {
        private ThirdPersonCameraConfigSO config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return InitScene();
            config = ScriptableObject.CreateInstance<ThirdPersonCameraConfigSO>();
        }

        [Test]
        public void NotModifyTheTransformWithoutActivation()
        {
            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 3 });
            var dummyTransform = new GameObject("_dummyTransform").transform;
            dummyTransform.position = Vector3.up * 1.5f;

            var cameraSetup = new ThirdPersonCameraSetup(dummyTransform, config);

            Assert.AreNotEqual(config.Get().offset, dummyTransform.position);
        }

        [Test]
        public void ModifyTheTransformOnActivation()
        {
            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 3 });
            var dummyTransform = new GameObject("_dummyTransform").transform;

            var cameraSetup = new ThirdPersonCameraSetup(dummyTransform, config);
            cameraSetup.Activate();

            Assert.AreEqual(config.Get().offset, dummyTransform.position);
        }

        [Test]
        public void ReactToChangesInConfig()
        {
            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 3 });
            var dummyTransform = new GameObject("_dummyTransform").transform;
            var cameraSetup = new ThirdPersonCameraSetup(dummyTransform, config);
            cameraSetup.Activate();

            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 6 });

            Assert.AreEqual(config.Get().offset, dummyTransform.position);
        }
    }
}