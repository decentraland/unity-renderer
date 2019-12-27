using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CharacterControllerTests : TestsBase
    {
        [UnityTest]
        public IEnumerator CharacterTeleportReposition()
        {
            DCLCharacterController.i.PauseGravity();
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
            DCLCharacterController.i.ResumeGravity();
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
                x = 50f + PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE,
                y = 2f,
                z = 50f + PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE
            }));

            yield return null;
            Assert.AreEqual(new Vector3(50f, 2f, 50f), DCLCharacterController.i.transform.position);

            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = -50f - PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE,
                y = 2f,
                z = -50f - PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE
            }));

            yield return null;
            Assert.AreEqual(new Vector3(-50f, 2f, -50f), DCLCharacterController.i.transform.position);
        }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator CharacterIsNotParentedOnWorldReposition()
        {
            // We use a shape that represents a static ground and has collisions
            TestHelpers.InstantiateEntityWithShape(scene, "groundShape", DCL.Models.CLASS_ID.PLANE_SHAPE, Vector3.zero);
            var shapeEntity = scene.entities["groundShape"];

            // Reposition ground shape to be on the world-reposition-limit
            TestHelpers.SetEntityTransform(scene, shapeEntity,
            new DCLTransform.Model
            {
                position = new Vector3(PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE, 1f, PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE),
                rotation = Quaternion.Euler(90f, 0f, 0f),
                scale = new Vector3(20, 20, 1)
            });

            // Place character on the ground shape and check if it's detected as ground
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE - 2f,
                y = 3f,
                z = PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE - 2f
            }));

            // Let the character *fall* onto the ground shape
            yield return new WaitForSeconds(2f);

            Assert.IsTrue(Reflection_GetField<Transform>(DCLCharacterController.i, "groundTransform") == shapeEntity.meshRootGameObject.transform);

            // Place the character barely passing the limits to trigger the world repositioning
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE + 1f,
                y = DCLCharacterController.i.transform.position.y,
                z = PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE + 1f
            }));

            yield return null;

            // check if the character got repositioned correctly
            Assert.AreEqual(new Vector3(1f, DCLCharacterController.i.transform.position.y, 1f), DCLCharacterController.i.transform.position);

            // check it's not parented but still has the same ground
            Assert.IsTrue(Reflection_GetField<Transform>(DCLCharacterController.i, "groundTransform") == shapeEntity.meshRootGameObject.transform);
            Assert.IsTrue(DCLCharacterController.i.transform.parent == null);
        }

        [UnityTest]
        public IEnumerator Character_UpdateSOPosition()
        {
            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.characterController.enabled = false;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 50f,
                y = 2f,
                z = 0f
            }));

            yield return null;

            Assert.AreEqual(new Vector3(50f, 2f, 0f), CommonScriptableObjects.playerUnityPosition);
            DCLCharacterController.i.ResumeGravity();
            DCLCharacterController.i.characterController.enabled = true;
        }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator Character_UpdateSORotation()
        {
            DCLCharacterController.i.PauseGravity();

            var newEulerAngle = 10f;
            CommonScriptableObjects.characterForward.Set(Quaternion.Euler(new Vector3(0, newEulerAngle, 0)) * Vector3.forward);
            Cursor.lockState = CursorLockMode.Locked;
            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual(DCLCharacterController.i.transform.eulerAngles, CommonScriptableObjects.playerUnityEulerAngles);
            DCLCharacterController.i.ResumeGravity();
        }

        [UnityTest]
        public IEnumerator CharacterSupportsMovingPlatforms()
        {
            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 2f,
                y = 3f,
                z = 8f
            }));
            yield return null;

            Assert.IsTrue(Vector3.Distance(DCLCharacterController.i.transform.position, new Vector3(2f, 3f, 8f)) < 0.1f);

            string platformEntityId = "movingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(2f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(2f, 0.5f, 2f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(2f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.ResumeGravity();

            // Let the character *fall* onto the platform
            yield return new WaitForSeconds(2f);

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character should parent itself with a platform only if it moves/rotates");

            // Lerp the platform's position
            float lerpTime = 0f;
            float lerpSpeed = 2f;
            Vector3 originalPosition = platformTransform.position;
            Vector3 targetPosition = new Vector3(10f, 1f, 8f);

            bool checkedParent = false;
            while (lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if (lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.position = Vector3.Lerp(originalPosition, targetPosition, lerpTime);

                if (!checkedParent && lerpTime >= 0.5f)
                {
                    Assert.IsNotNull(DCLCharacterController.i.transform.parent, "The character should be parented to the moving platform");

                    checkedParent = true;
                }
            }

            // check positions
            Assert.IsTrue(Vector3.Distance(platformTransform.position, targetPosition) < 0.1f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.x, targetPosition.x, 0.5f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.z, targetPosition.z, 0.5f);
        }

        [UnityTest]
        public IEnumerator CharacterSupportsRotatingPlatforms()
        {
            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 5f,
                y = 3f,
                z = 5f
            }));
            yield return null;

            Assert.IsTrue(Vector3.Distance(DCLCharacterController.i.transform.position, new Vector3(5f, 3f, 5f)) < 0.1f);

            string platformEntityId = "rotatingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(8f, 0.5f, 8f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(8f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.ResumeGravity();

            // Let the character *fall* onto the platform
            yield return new WaitForSeconds(2f);

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character should parent itself with a platform only if it moves/rotates");

            // Lerp the platform's rotation
            float lerpTime = 0f;
            float lerpSpeed = 1f;
            Quaternion initialRotation = Quaternion.identity;
            Quaternion targetRotation = Quaternion.Euler(0, 180f, 0f);

            bool checkedParent = false;
            while (lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if (lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, lerpTime);

                if (!checkedParent && lerpTime >= 0.5f)
                {
                    Assert.IsNotNull(DCLCharacterController.i.transform.parent, "The character should be parented to the rotating platform");

                    checkedParent = true;
                }
            }

            // check positions
            Assert.IsTrue(Vector3.Distance(platformTransform.rotation.eulerAngles, targetRotation.eulerAngles) < 0.1f);

            UnityEngine.Assertions.Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.x, 11f, 0.5f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.z, 11f, 0.5f);

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
        [NUnit.Framework.Explicit("This test is failing. May be related to the new camera setup, please check MainTest scene")]
        [Category("Explicit")]
        public IEnumerator CharacterIsReleasedOnFastPlatform()
        {
            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 5f,
                y = 3f,
                z = 5f
            }));
            yield return null;

            Assert.IsTrue(Vector3.Distance(DCLCharacterController.i.transform.position, new Vector3(5f, 3f, 5f)) < 0.1f);

            string platformEntityId = "rotatingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(8f, 0.5f, 8f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(8f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.ResumeGravity();

            // Let the character *fall* onto the platform
            yield return new WaitForSeconds(2f);

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character should parent itself with a platform only if it moves/rotates");

            // Lerp the platform's rotation
            float lerpTime = 0f;
            float lerpSpeed = 1f;
            Quaternion initialRotation = Quaternion.identity;
            Quaternion targetRotation = Quaternion.Euler(0, 180f, 0f);

            bool checkedParent = false;
            while (lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if (lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, lerpTime);

                if (!checkedParent && lerpTime >= 0.5f)
                {
                    Assert.IsNotNull(DCLCharacterController.i.transform.parent, "The character should be parented to the rotating platform");

                    checkedParent = true;
                }
            }

            // Accelerate platform
            lerpTime = 0f;
            lerpSpeed = 15f;
            initialRotation = platformTransform.rotation;
            targetRotation = Quaternion.Euler(0, 360f, 0f);
            while (lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if (lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, lerpTime);
            }

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character shouldn't be parented anymore");
        }
    }
}
