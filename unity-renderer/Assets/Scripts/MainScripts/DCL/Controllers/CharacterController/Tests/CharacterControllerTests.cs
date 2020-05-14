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
            yield return InitCharacterPosition(10, 2, 10);
        }

        public IEnumerator InitCharacterPosition(float x, float y, float z, bool pauseGravity = true)
        {
            yield return InitCharacterPosition(new Vector3(x, y, z), pauseGravity);
        }

        public IEnumerator InitCharacterPosition(Vector3 position, bool pauseGravity = true)
        {
            if (pauseGravity)
                DCLCharacterController.i.PauseGravity();
            else
                DCLCharacterController.i.ResumeGravity();

            DCLCharacterController.i.Teleport(JsonUtility.ToJson(position));

            UnityEngine.Assertions.Assert.AreApproximatelyEqual(0, Vector3.Distance(DCLCharacterController.i.characterPosition.worldPosition, position), 2.0f);

            yield return null;
        }

        public IEnumerator WaitUntilGrounded()
        {
            // Let the character *fall* onto the ground shape
            yield return new WaitUntil(() => DCLCharacterController.i.isGrounded);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterAdjustPosition()
        {
            Vector3 originalCharacterPosition = new Vector3
            {
                x = 50f,
                y = 2f,
                z = 0f
            };

            yield return InitCharacterPosition(originalCharacterPosition, true);

            var pos2 = new Vector3
            {
                x = 50f + PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE,
                y = 2f,
                z = 50f + PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE
            };

            yield return InitCharacterPosition(pos2, true);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(0, Vector3.Distance(new Vector3(50f, 2f, 50f), DCLCharacterController.i.transform.position), 0.5f);

            var pos3 = new Vector3
            {
                x = -50f - PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE,
                y = 2f,
                z = -50f - PlayerSettings.WORLD_REPOSITION_MINIMUM_DISTANCE
            };

            yield return InitCharacterPosition(pos3, true);
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

            yield return WaitUntilGrounded();

            Assert.IsTrue(DCLCharacterController.i.groundTransform == shapeEntity.meshRootGameObject.transform);

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
            Assert.IsTrue(DCLCharacterController.i.groundTransform == shapeEntity.meshRootGameObject.transform);
            Assert.IsTrue(DCLCharacterController.i.transform.parent == null);
        }

        [UnityTest]
        public IEnumerator Character_UpdateSOPosition()
        {
            yield return InitCharacterPosition(50, 2, 0);
            Assert.AreEqual(new Vector3(50f, 2f, 0f), CommonScriptableObjects.playerUnityPosition);
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
        [NUnit.Framework.Explicit("This test started failing on the CI out of the blue. Will be re-enabled after implementing a solution dealing with high delta times")]
        [Category("Explicit")]
        public IEnumerator CharacterSupportsMovingPlatforms()
        {
            Vector3 originalCharacterPosition = new Vector3
            {
                x = 2f,
                y = 3f,
                z = 8f
            };

            yield return InitCharacterPosition(originalCharacterPosition);

            string platformEntityId = "movingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(2f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(2f, 0.5f, 2f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(2f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.ResumeGravity();

            yield return WaitUntilGrounded();

            Assert.IsFalse(DCLCharacterController.i.isOnMovingPlatform, "isOnMovingPlatform should be true only if the platform moves/rotates");

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

                DCLCharacterController.i.LateUpdate();

                if (!checkedParent && lerpTime >= 0.25f)
                {
                    Assert.IsTrue(DCLCharacterController.i.isOnMovingPlatform, "isOnMovingPlatform should be true when the platform moves/rotates");
                    checkedParent = true;
                }
            }

            // check positions
            Assert.IsTrue(Vector3.Distance(platformTransform.position, targetPosition) < 0.1f);

            float dist1 = Vector3.Distance(originalCharacterPosition, DCLCharacterController.i.transform.position);
            float dist2 = Vector3.Distance(originalPosition, targetPosition);

            UnityEngine.Assertions.Assert.AreApproximatelyEqual(dist1, dist2, 1f);
        }


        [UnityTest]
        [NUnit.Framework.Explicit("This test started failing on the CI out of the blue. Will be re-enabled after implementing a solution dealing with high delta times")]
        [Category("Explicit")]
        public IEnumerator CharacterSupportsRotatingPlatforms()
        {
            Vector3 originalCharacterPosition = new Vector3
            {
                x = 5f,
                y = 3f,
                z = 5f
            };

            yield return InitCharacterPosition(originalCharacterPosition);

            string platformEntityId = "rotatingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(8f, 0.5f, 8f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(8f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.ResumeGravity();

            yield return WaitUntilGrounded();

            Assert.IsFalse(DCLCharacterController.i.isOnMovingPlatform, "isOnMovingPlatform should be true only if the platform moves/rotates");

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
                    Assert.IsTrue(DCLCharacterController.i.isOnMovingPlatform, "isOnMovingPlatform should be true when the platform moves/rotates");

                    checkedParent = true;
                }
            }

            // check positions
            Assert.IsTrue(Vector3.Distance(platformTransform.rotation.eulerAngles, targetRotation.eulerAngles) < 0.1f);

            UnityEngine.Assertions.Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.x, 11f, 1f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(DCLCharacterController.i.transform.position.z, 11f, 1f);

            // remove platform and check character parent
            TestHelpers.RemoveSceneEntity(scene, platformEntityId);
            yield return null;

            Assert.IsFalse(DCLCharacterController.i.isOnMovingPlatform, "isOnMovingPlatform should be false as there's no platform anymore");
        }

        [UnityTest]
        [NUnit.Framework.Explicit("This test started failing on the CI out of the blue. Will be re-enabled after implementing a solution dealing with high delta times")]
        [Category("Explicit")]
        public IEnumerator CharacterIsReleasedOnEntityRemoval()
        {
            yield return CharacterSupportsMovingPlatforms();

            // remove platform and check character parent
            string platformEntityId = "movingPlatform";
            TestHelpers.RemoveSceneEntity(scene, platformEntityId);
            yield return null;
            yield return null;
            yield return null;

            Assert.IsNull(DCLCharacterController.i.transform.parent, "The character shouldn't be parented as there's no platform anymore");
        }

        [UnityTest]
        [NUnit.Framework.Explicit("This test started failing on the CI out of the blue. Will be re-enabled after implementing a solution dealing with high delta times")]
        [Category("Explicit")]
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
        [NUnit.Framework.Explicit("This test started failing on the CI out of the blue. Will be re-enabled after implementing a solution dealing with high delta times")]
        [Category("Explicit")]
        public IEnumerator CharacterIsReleasedOnShapeRemoval()
        {
            yield return CharacterSupportsMovingPlatforms();

            // remove shape component
            string platformEntityId = "movingPlatform";
            var shapeComponent = scene.entities[platformEntityId].GetSharedComponent(typeof(BaseShape));
            TestHelpers.DetachSharedComponent(scene, platformEntityId, shapeComponent.id);

            yield return null;
            yield return null;

            Assert.IsFalse(DCLCharacterController.i.isOnMovingPlatform, "isOnMovingPlatform should be false when the shape colliders are disabled");
        }

        [UnityTest]
        [NUnit.Framework.Explicit("This test is failing. May be related to the new camera setup, please check MainTest scene")]
        [Category("Explicit")]
        public IEnumerator CharacterIsReleasedOnFastPlatform()
        {
            Vector3 initialCharacterPosition = new Vector3(5, 3, 5);
            yield return InitCharacterPosition(initialCharacterPosition);

            string platformEntityId = "rotatingPlatform";
            TestHelpers.InstantiateEntityWithShape(scene, platformEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8f, 1f, 8f));

            Transform platformTransform = scene.entities[platformEntityId].gameObject.transform;
            platformTransform.localScale = new Vector3(8f, 0.5f, 8f);

            yield return null;
            Assert.IsTrue(Vector3.Distance(platformTransform.position, new Vector3(8f, 1f, 8f)) < 0.1f);

            // enable character gravity
            DCLCharacterController.i.ResumeGravity();

            yield return WaitUntilGrounded();

            Assert.IsFalse(DCLCharacterController.i.isOnMovingPlatform, "isOnMovingPlatform should be true only if the platform moves/rotates");

            // Lerp the platform's rotation
            float lerpTime = 0f;
            float lerpSpeed = 1f;
            Quaternion initialRotation = Quaternion.identity;
            Quaternion targetRotation = Quaternion.Euler(0, 180f, 0f);

            bool checkedIsOnMovingTransform = false;
            while (lerpTime < 1f)
            {
                yield return null;
                lerpTime += Time.deltaTime * lerpSpeed;

                if (lerpTime > 1f)
                    lerpTime = 1f;

                platformTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, lerpTime);

                if (!checkedIsOnMovingTransform && lerpTime >= 0.5f)
                {
                    Assert.IsTrue(DCLCharacterController.i.isOnMovingPlatform);
                    checkedIsOnMovingTransform = true;
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

            Assert.IsTrue(DCLCharacterController.i.isOnMovingPlatform);
        }
    }
}
