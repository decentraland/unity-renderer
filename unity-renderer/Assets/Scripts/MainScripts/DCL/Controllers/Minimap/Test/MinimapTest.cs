using System.Collections;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class MinimapTest : TestsBase
    {
        [UnityTest]
        public IEnumerator MinimapCamera_Position()
        {
            yield return InitScene();

            var minimapCamera = InstantiateMinimapCam();
            minimapCamera.transform.position = Vector3.zero;
            yield return null;

            var newPos = new Vector3(10, 0, 10);
            CommonScriptableObjects.playerUnityPosition.Set(newPos);
            yield return null;

            Assert.AreApproximatelyEqual(newPos.x, minimapCamera.transform.position.x);
            Assert.AreApproximatelyEqual(newPos.z, minimapCamera.transform.position.z);
        }

        [UnityTest]
        public IEnumerator MinimapCamera_RotationNorthLocked()
        {
            yield return InitScene();

            var minimapCamera = InstantiateMinimapCam();
            minimapCamera.transform.eulerAngles = Vector3.zero;
            minimapCamera.northLocked = true;
            yield return null;

            var newRotation = 10f;
            CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.up * newRotation);
            yield return null;

            Assert.AreEqual(Vector3.zero, minimapCamera.transform.eulerAngles);
        }

        [UnityTest]
        public IEnumerator MinimapCamera_RotationNorthNotLocked()
        {
            yield return InitScene();

            var minimapCamera = InstantiateMinimapCam();
            minimapCamera.transform.eulerAngles = Vector3.zero;
            minimapCamera.northLocked = false;
            yield return null;

            var newRotation = 50f;
            CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.up * newRotation);
            yield return null;


            Assert.AreApproximatelyEqual(minimapCamera.transform.eulerAngles.y, CommonScriptableObjects.playerUnityEulerAngles.Get().y);
        }

        MinimapCamera InstantiateMinimapCam()
        {
            GameObject minimapGameObject = GameObject.Instantiate(Resources.Load<GameObject>("Minimap"));
            var minimapCamera = minimapGameObject.GetComponentInChildren<MinimapCamera>();

            return minimapCamera;
        }
    }
}