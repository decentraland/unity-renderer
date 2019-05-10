using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Newtonsoft.Json;
using NUnit.Framework;
using DCL.Helpers;
using DCL.Models;
using DCL.Components;

namespace Tests
{
    public class CharacterControllerTests
    {
        // TODO: Find a way to run this test on Unity Cloud Build, even though it passes locally, it fails on timeout in Unity Cloud Build
        [UnityTest]
        public IEnumerator CharacterTeleportReposition()
        {
            yield return TestHelpers.UnloadAllUnityScenes();
            var characterController = (GameObject.Instantiate(Resources.Load("Prefabs/CharacterController") as GameObject)).GetComponent<DCLCharacterController>();
            characterController.gravity = 0f;

            Assert.AreEqual(new Vector3(0f, 0f, 0f), characterController.transform.position);

            characterController.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 10f,
                y = 0f,
                z = 0f
            }));

            yield return new WaitForEndOfFrame();

            Assert.AreEqual(new Vector3(10f, 3f, 0f), characterController.transform.position);
        }
    }
}
