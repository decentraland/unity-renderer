using DCL.Helpers;
using DCL.Models;
using DCL.Components;
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
            DCLCharacterController.i.characterController.enabled = false;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 10f,
                y = 2f,
                z = 10f
            }));

            yield return null;

            Assert.AreEqual(new Vector3(10f, 2f, 10f), DCLCharacterController.i.transform.position);
        }

        [UnityTest]
        public IEnumerator CharacterAdjustPosition()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;
            DCLCharacterController.i.characterController.enabled = false;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 50f,
                y = 2f,
                z = 0f
            }));

            yield return null;
            Assert.AreEqual(new Vector3(50f, 2f, 0f), DCLCharacterController.i.transform.position);

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 50f + DCLCharacterPosition.LIMIT,
                y = 2f,
                z = 50f + DCLCharacterPosition.LIMIT
            }));

            yield return null;
            Assert.AreEqual(new Vector3(50f, 2f, 50f), DCLCharacterController.i.transform.position);

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = -50f - DCLCharacterPosition.LIMIT,
                y = 2f,
                z = -50f - DCLCharacterPosition.LIMIT
            }));

            yield return null;
            Assert.AreEqual(new Vector3(-50f, 2f, -50f), DCLCharacterController.i.transform.position);
        }

        [UnityTest]
        public IEnumerator Character_UpdateSOPosition()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;
            DCLCharacterController.i.characterController.enabled = false;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 50f,
                y = 2f,
                z = 0f
            }));

            yield return null;

            Assert.AreEqual(new Vector3(50f, 2f, 0f), CommonScriptableObjects.playerUnityPosition);
        }

        [UnityTest]
        public IEnumerator Character_UpdateSORotation()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            var newEulerAngle = 10f;
            DCLCharacterController.i.GetType().GetField("aimingHorizontalDeltaAngle", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(DCLCharacterController.i, newEulerAngle);
            Cursor.lockState = CursorLockMode.Locked;
            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual(DCLCharacterController.i.transform.eulerAngles, CommonScriptableObjects.playerUnityEulerAngles);
        }

        [UnityTest]
        public IEnumerator CharacterSupportsMovingPlatforms()
        {
            yield return base.InitScene();

            float originalGravity = DCLCharacterController.i.gravity;
            DCLCharacterController.i.gravity = 0f;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 2f,
                y = 3f,
                z = 8f
            }));
            yield return null;

            Assert.IsTrue(Vector3.Distance(DCLCharacterController.i.transform.position,  new Vector3(2f, 3f, 8f)) < 0.1f);

            string platformEntityId = "movingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(2f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(2f, 0.5f, 2f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(2f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.gravity = originalGravity;

            // Let the character *fall* onto the platform
            yield return new WaitForSeconds(2f);

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character should parent itself with a platform only if it moves/rotates");

            // Lerp the platform's position
            float lerpTime = 0f;
            float lerpSpeed = 2f;
            Vector3 originalPosition = platformTransform.position;
            Vector3 targetPosition = new Vector3(10f, 1f, 8f);

            bool checkedParent = false;
            while(lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if(lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.position = Vector3.Lerp(originalPosition, targetPosition, lerpTime);

                if(!checkedParent && lerpTime >= 0.5f)
                {
                    Assert.IsNotNull(DCLCharacterController.i.transform.parent, "The character should be parented to the moving platform");

                    checkedParent = true;
                }
            }

            // check positions
            Assert.IsTrue(Vector3.Distance(platformTransform.position, targetPosition) < 0.1f);
            Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.x, targetPosition.x, 0.5f);
            Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.z, targetPosition.z, 0.5f);
        }

        [UnityTest]
        public IEnumerator CharacterSupportsRotatingPlatforms()
        {
            yield return base.InitScene();

            float originalGravity = DCLCharacterController.i.gravity;
            DCLCharacterController.i.gravity = 0f;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 5f,
                y = 3f,
                z = 5f
            }));
            yield return null;

            Assert.IsTrue(Vector3.Distance(DCLCharacterController.i.transform.position,  new Vector3(5f, 3f, 5f)) < 0.1f);

            string platformEntityId = "rotatingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(8f, 0.5f, 8f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(8f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.gravity = originalGravity;

            // Let the character *fall* onto the platform
            yield return new WaitForSeconds(2f);

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character should parent itself with a platform only if it moves/rotates");

            // Lerp the platform's rotation
            float lerpTime = 0f;
            float lerpSpeed = 1f;
            Quaternion initialRotation = Quaternion.identity;
            Quaternion targetRotation = Quaternion.Euler(0, 180f, 0f);

            bool checkedParent = false;
            while(lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if(lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, lerpTime);

                if(!checkedParent && lerpTime >= 0.5f)
                {
                    Assert.IsNotNull(DCLCharacterController.i.transform.parent, "The character should be parented to the rotating platform");

                    checkedParent = true;
                }
            }

            // check positions
            Assert.IsTrue(Vector3.Distance(platformTransform.rotation.eulerAngles, targetRotation.eulerAngles) < 0.1f);

            Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.x, 11f, 0.5f);
            Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.z, 11f, 0.5f);

            // remove platform and check character parent
            TestHelpers.RemoveSceneEntity(scene, platformEntityId);
            yield return null;

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character shouldn't be parented as there's no platform anymore");
        }

        [UnityTest]
        public IEnumerator CharacterIsReleasedOnEntityRemoval()
        {
            yield return CharacterSupportsMovingPlatforms();

            // remove platform and check character parent
            string platformEntityId = "movingPlatform";
            TestHelpers.RemoveSceneEntity(scene, platformEntityId);
            yield return null;

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character shouldn't be parented as there's no platform anymore");
        }

        [UnityTest]
        public IEnumerator CharacterIsReleasedOnPlatformCollisionToggle()
        {
           yield return CharacterSupportsMovingPlatforms();

            // Disable shape colliders
            string platformEntityId = "movingPlatform";
            var shapeComponent = scene.entities[platformEntityId].GetSharedComponent(typeof(BaseShape));
            yield return TestHelpers.SharedComponentUpdate(shapeComponent, new BaseShape.Model()
            {
                withCollisions = false
            });

            yield return null;
            yield return null;

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character shouldn't be parented as the shape colliders were disabled");
        }

        [UnityTest]
        public IEnumerator CharacterIsReleasedOnShapeRemoval()
        {
           yield return CharacterSupportsMovingPlatforms();

            // remove shape component
            string platformEntityId = "movingPlatform";
            var shapeComponent = scene.entities[platformEntityId].GetSharedComponent(typeof(BaseShape));
            TestHelpers.DetachSharedComponent(scene, platformEntityId, shapeComponent.id);

            yield return null;
            yield return null;

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character shouldn't be parented as the shape colliders were disabled");
        }

        [UnityTest]
        public IEnumerator CharacterIsReleasedOnFastPlatform()
        {
            yield return base.InitScene();

            float originalGravity = DCLCharacterController.i.gravity;
            DCLCharacterController.i.gravity = 0f;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 5f,
                y = 3f,
                z = 5f
            }));
            yield return null;

            Assert.IsTrue(Vector3.Distance(DCLCharacterController.i.transform.position,  new Vector3(5f, 3f, 5f)) < 0.1f);

            string platformEntityId = "rotatingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(8f, 0.5f, 8f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(8f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.gravity = originalGravity;

            // Let the character *fall* onto the platform
            yield return new WaitForSeconds(2f);

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character should parent itself with a platform only if it moves/rotates");

            // Lerp the platform's rotation
            float lerpTime = 0f;
            float lerpSpeed = 1f;
            Quaternion initialRotation = Quaternion.identity;
            Quaternion targetRotation = Quaternion.Euler(0, 180f, 0f);

            bool checkedParent = false;
            while(lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if(lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, lerpTime);

                if(!checkedParent && lerpTime >= 0.5f)
                {
                    Assert.IsNotNull(DCLCharacterController.i.transform.parent, "The character should be parented to the rotating platform");

                    checkedParent = true;
                }
            }

            // Accelerate platform
            lerpTime = 0f;
            lerpSpeed = 10f;
            initialRotation = platformTransform.rotation;
            targetRotation = Quaternion.Euler(0, 360f, 0f);
            while(lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if(lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, lerpTime);
            }

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character shouldn't be parented anymore");
        }
    }
}
