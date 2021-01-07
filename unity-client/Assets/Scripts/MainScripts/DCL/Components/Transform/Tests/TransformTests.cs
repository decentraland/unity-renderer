using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TransformTests : IntegrationTestSuite_Legacy
    {
        [Test]
        public void TransformUpdate()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            Assert.IsTrue(entity != null);

            {
                Vector3 originalTransformPosition = entity.gameObject.transform.position;
                Quaternion originalTransformRotation = entity.gameObject.transform.rotation;
                Vector3 originalTransformScale = entity.gameObject.transform.localScale;

                Vector3 position = new Vector3(5, 1, 5);
                Quaternion rotationQuaternion = Quaternion.Euler(10, 50, -90);
                Vector3 scale = new Vector3(0.7f, 0.7f, 0.7f);

                var transformModel = new DCLTransform.Model { position = position, rotation = rotationQuaternion, scale = scale };

                TestHelpers.SetEntityTransform(scene, entity, transformModel);

                Assert.AreNotEqual(originalTransformPosition, entity.gameObject.transform.position);
                Assert.AreEqual(position, entity.gameObject.transform.position);

                Assert.AreNotEqual(originalTransformRotation, entity.gameObject.transform.rotation);
                Assert.AreEqual(rotationQuaternion.ToString(), entity.gameObject.transform.rotation.ToString());

                Assert.AreNotEqual(originalTransformScale, entity.gameObject.transform.localScale);
                Assert.AreEqual(scale, entity.gameObject.transform.localScale);
            }

            {
                Vector3 originalTransformPosition = entity.gameObject.transform.position;
                Quaternion originalTransformRotation = entity.gameObject.transform.rotation;
                Vector3 originalTransformScale = entity.gameObject.transform.localScale;

                Vector3 position = new Vector3(51, 13, 52);
                Quaternion rotationQuaternion = Quaternion.Euler(101, 51, -91);
                Vector3 scale = new Vector3(1.7f, 3.7f, -0.7f);

                var transformModel = new DCLTransform.Model { position = position, rotation = rotationQuaternion, scale = scale };

                TestHelpers.SetEntityTransform(scene, entity, transformModel);

                Assert.AreNotEqual(originalTransformPosition, entity.gameObject.transform.position);
                Assert.AreEqual(position, entity.gameObject.transform.position);

                Assert.AreNotEqual(originalTransformRotation, entity.gameObject.transform.rotation);
                Assert.AreEqual(rotationQuaternion.ToString(), entity.gameObject.transform.rotation.ToString());

                Assert.AreNotEqual(originalTransformScale, entity.gameObject.transform.localScale);
                Assert.AreEqual(scale, entity.gameObject.transform.localScale);
            }

            {
                Vector3 originalTransformPosition = entity.gameObject.transform.position;
                Quaternion originalTransformRotation = entity.gameObject.transform.rotation;
                Vector3 originalTransformScale = entity.gameObject.transform.localScale;

                Vector3 position = new Vector3(0, 0, 0);
                Quaternion rotationQuaternion = Quaternion.Euler(0, 0, 0);
                Vector3 scale = new Vector3(1, 1, 1);

                var transformModel = new DCLTransform.Model { position = position, rotation = rotationQuaternion, scale = scale };

                TestHelpers.SetEntityTransform(scene, entity, transformModel);

                Assert.AreNotEqual(originalTransformPosition, entity.gameObject.transform.position);
                Assert.AreEqual(position, entity.gameObject.transform.position);

                Assert.AreNotEqual(originalTransformRotation, entity.gameObject.transform.rotation);
                Assert.AreEqual(rotationQuaternion.ToString(), entity.gameObject.transform.rotation.ToString());

                Assert.AreNotEqual(originalTransformScale, entity.gameObject.transform.localScale);
                Assert.AreEqual(scale, entity.gameObject.transform.localScale);
            }
        }

        [Test]
        public void TransformationsAreKeptRelativeAfterParenting()
        {


            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            Vector3 targetPosition = new Vector3(3f, 7f, 1f);
            Quaternion targetRotation = new Quaternion(4f, 9f, 1f, 7f);
            Vector3 targetScale = new Vector3(5f, 0.7f, 2f);

            // 1. Create component with non-default configs
            DCLTransform.Model componentModel = new DCLTransform.Model
            {
                position = targetPosition,
                rotation = targetRotation,
                scale = targetScale
            };

            TestHelpers.SetEntityTransform(scene, entity, componentModel);

            // 2. Check configured values
            Assert.IsTrue(targetPosition == entity.gameObject.transform.localPosition);
            Assert.IsTrue(targetRotation == entity.gameObject.transform.localRotation);
            Assert.IsTrue(targetScale == entity.gameObject.transform.localScale);

            // 3. Create new parent entity
            DecentralandEntity entity2 = TestHelpers.CreateSceneEntity(scene);

            componentModel = new DCLTransform.Model
            {
                position = new Vector3(15f, 56f, 0f),
                rotation = new Quaternion(1f, 3f, 5f, 15f),
                scale = new Vector3(2f, 3f, 5f)
            };

            TestHelpers.SetEntityTransform(scene, entity2, componentModel);

            // 4. set new parent
            TestHelpers.SetEntityParent(scene, entity.entityId, entity2.entityId);

            // 5. check transform values remains the same
            Assert.IsTrue(targetPosition == entity.gameObject.transform.localPosition);
            Assert.IsTrue(targetRotation == entity.gameObject.transform.localRotation);
            Assert.IsTrue(targetScale == entity.gameObject.transform.localScale);
        }
    }
}
