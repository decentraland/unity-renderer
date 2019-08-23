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
            InitScene();
            var minimapCamera = new GameObject().AddComponent<MinimapCamera>();
            minimapCamera.transform.position = Vector3.zero;
            yield return null;

            var newPos = new Vector3(10, 0, 10);
            CommonScriptableObjects.playerUnityPosition.Set(newPos);
            yield return null;

            Assert.AreEqual(newPos, minimapCamera.transform.position);
        }

        [UnityTest]
        public IEnumerator MinimapCamera_RotationNorthLocked()
        {
            InitScene();
            var minimapCamera = new GameObject().AddComponent<MinimapCamera>();
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
            InitScene();
            var minimapCamera = new GameObject().AddComponent<MinimapCamera>();
            minimapCamera.transform.eulerAngles = Vector3.zero;
            minimapCamera.northLocked = false;
            yield return null;

            var newRotation = 10f;
            CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.up * newRotation);
            yield return null;

            Assert.AreApproximatelyEqual(minimapCamera.transform.eulerAngles.y, CommonScriptableObjects.playerUnityEulerAngles.Get().y);
        }
    }
}