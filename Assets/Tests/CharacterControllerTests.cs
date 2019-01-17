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
    public class CharacterControllerTests : MonoBehaviour
    {
        [UnityTest]
        public IEnumerator CharacterTeleportReposition()
        {
            var characterController = (Resources.Load("Prefabs/CharacterController") as GameObject).GetComponent<DCLCharacterController>();
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