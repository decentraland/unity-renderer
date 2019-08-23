using DCL.Helpers;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class CharacterControllerTests : TestsBase
    {
        // TODO: Find a way to run this test on Unity Cloud Build, even though it passes locally, it fails on timeout in Unity Cloud Build
        [UnityTest]
        public IEnumerator CharacterTeleportReposition()
        {
            yield return base.InitScene();

            DCLCharacterController.i.gravity = 0f;

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 10f,
                y = 0f,
                z = 0f
            }));

            yield return null;

            Assert.IsTrue(new Vector3(10f, 0f, 0f) == DCLCharacterController.i.transform.position);
        }

        [UnityTest]
        public IEnumerator CharacterAdjustPosition()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 50f,
                y = 0f,
                z = 0f
            }));

            yield return null;

            Assert.AreEqual(new Vector3(50f, 0f, 0f), DCLCharacterController.i.transform.position);

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 50f + DCLCharacterPosition.LIMIT,
                y = 0f,
                z = 50f + DCLCharacterPosition.LIMIT
            }));

            yield return null;

            Assert.AreEqual(new Vector3(50f, 0f, 50f), DCLCharacterController.i.transform.position);

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = -50f - DCLCharacterPosition.LIMIT,
                y = 0f,
                z = -50f - DCLCharacterPosition.LIMIT
            }));

            yield return null;

            Assert.AreEqual(new Vector3(-50f, 0f, -50f), DCLCharacterController.i.transform.position);
        }

        [UnityTest]
        public IEnumerator Character_UpdateSOPosition()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 50f,
                y = 0f,
                z = 0f
            }));

            yield return null;

            Assert.AreEqual(new Vector3(50f, 0f, 0f), CommonScriptableObjects.playerUnityPosition);
        }

        [UnityTest]
        public IEnumerator Character_UpdateSORotation()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            var newEulerAngle = 10f;
            DCLCharacterController.i.GetType().GetField("aimingHorizontalAngle", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(DCLCharacterController.i, newEulerAngle);
            Cursor.lockState = CursorLockMode.Locked;
            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual(DCLCharacterController.i.transform.eulerAngles, CommonScriptableObjects.playerUnityEulerAngles);
        }
    }
}
