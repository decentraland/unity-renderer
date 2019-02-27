using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Tests
{
    public class TransformTests
    {
        [UnityTest]
        public IEnumerator TransformUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(sceneController != null);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.IsTrue(scene != null);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            var entityObject = scene.entities[entityId];

            Assert.IsTrue(entityObject != null);

            {
                Vector3 originalTransformPosition = entityObject.gameObject.transform.position;
                Quaternion originalTransformRotation = entityObject.gameObject.transform.rotation;
                Vector3 originalTransformScale = entityObject.gameObject.transform.localScale;

                Vector3 position = new Vector3(5, 1, 5);
                Quaternion rotationQuaternion = Quaternion.Euler(10, 50, -90);
                Vector3 scale = new Vector3(0.7f, 0.7f, 0.7f);

                string rawJSON = JsonConvert.SerializeObject(new
                {
                    entityId = entityId,
                    name = "transform",
                    classId = CLASS_ID_COMPONENT.TRANSFORM,
                    json = JsonConvert.SerializeObject(new
                    {
                        position = position,
                        rotation = new
                        {
                            x = rotationQuaternion.x,
                            y = rotationQuaternion.y,
                            z = rotationQuaternion.z,
                            w = rotationQuaternion.w
                        },
                        scale = scale
                    })
                });

                Assert.IsTrue(!string.IsNullOrEmpty(rawJSON));

                scene.EntityComponentCreate(rawJSON);

                Assert.AreNotEqual(originalTransformPosition, entityObject.gameObject.transform.position);
                Assert.AreEqual(position, entityObject.gameObject.transform.position);

                Assert.AreNotEqual(originalTransformRotation, entityObject.gameObject.transform.rotation);
                Assert.AreEqual(rotationQuaternion.ToString(), entityObject.gameObject.transform.rotation.ToString());

                Assert.AreNotEqual(originalTransformScale, entityObject.gameObject.transform.localScale);
                Assert.AreEqual(scale, entityObject.gameObject.transform.localScale);
            }


            {
                Vector3 originalTransformPosition = entityObject.gameObject.transform.position;
                Quaternion originalTransformRotation = entityObject.gameObject.transform.rotation;
                Vector3 originalTransformScale = entityObject.gameObject.transform.localScale;

                Vector3 position = new Vector3(51, 13, 52);
                Quaternion rotationQuaternion = Quaternion.Euler(101, 51, -91);
                Vector3 scale = new Vector3(1.7f, 3.7f, -0.7f);

                string rawJSON = JsonConvert.SerializeObject(new EntityComponentCreateMessage
                {
                    entityId = entityId,
                    name = "transform",
                    classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
                    json = JsonConvert.SerializeObject(new
                    {
                        position = position,
                        rotation = new
                        {
                            x = rotationQuaternion.x,
                            y = rotationQuaternion.y,
                            z = rotationQuaternion.z,
                            w = rotationQuaternion.w
                        },
                        scale = scale
                    })
                });

                Assert.IsTrue(!string.IsNullOrEmpty(rawJSON));

                scene.EntityComponentCreate(rawJSON);

                Assert.AreNotEqual(originalTransformPosition, entityObject.gameObject.transform.position);
                Assert.AreEqual(position, entityObject.gameObject.transform.position);

                Assert.AreNotEqual(originalTransformRotation, entityObject.gameObject.transform.rotation);
                Assert.AreEqual(rotationQuaternion.ToString(), entityObject.gameObject.transform.rotation.ToString());

                Assert.AreNotEqual(originalTransformScale, entityObject.gameObject.transform.localScale);
                Assert.AreEqual(scale, entityObject.gameObject.transform.localScale);
            }

            {
                Vector3 originalTransformPosition = entityObject.gameObject.transform.position;
                Quaternion originalTransformRotation = entityObject.gameObject.transform.rotation;
                Vector3 originalTransformScale = entityObject.gameObject.transform.localScale;

                Vector3 position = new Vector3(0, 0, 0);
                Quaternion rotationQuaternion = Quaternion.Euler(0, 0, 0);
                Vector3 scale = new Vector3(1, 1, 1);

                string rawJSON = JsonUtility.ToJson(new EntityComponentRemoveMessage
                {
                    entityId = entityId,
                    name = "transform"
                });

                Assert.IsTrue(!string.IsNullOrEmpty(rawJSON));

                scene.EntityComponentRemove(rawJSON);

                yield return new WaitForSeconds(0.01f);

                Assert.AreNotEqual(originalTransformPosition, entityObject.gameObject.transform.position);
                Assert.AreEqual(position, entityObject.gameObject.transform.position);

                Assert.AreNotEqual(originalTransformRotation, entityObject.gameObject.transform.rotation);
                Assert.AreEqual(rotationQuaternion.ToString(), entityObject.gameObject.transform.rotation.ToString());

                Assert.AreNotEqual(originalTransformScale, entityObject.gameObject.transform.localScale);
                Assert.AreEqual(scale, entityObject.gameObject.transform.localScale);
            }
        }

        [UnityTest]
        public IEnumerator TransformComponentMissingValuesGetDefaultedOnUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForEndOfFrame();

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForEndOfFrame();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new DCLTransform.Model
            {
                position = new Vector3(3f, 7f, 1f),
                rotation = new Quaternion(4f, 9f, 1f, 7f),
                scale = new Vector3(5f, 0.7f, 2f)
            });

            DCLTransform transformComponent = (DCLTransform)scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "animation",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TRANSFORM,
                json = componentJSON
            }));

            // 2. Check configured values
            Assert.AreEqual(new Vector3(3f, 7f, 1f), transformComponent.model.position);
            Assert.AreEqual(new Quaternion(4f, 9f, 1f, 7f), transformComponent.model.rotation);
            Assert.AreEqual(new Vector3(5f, 0.7f, 2f), transformComponent.model.scale);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new DCLTransform.Model
            {
                position = new Vector3(30f, 70f, 10f)
            });

            scene.EntityComponentUpdate(scene.entities[entityId], CLASS_ID_COMPONENT.TRANSFORM, componentJSON);

            // 4. Check changed values
            Assert.AreEqual(new Vector3(30f, 70f, 10f), transformComponent.model.position);

            // 5. Check defaulted values
            Assert.AreEqual(Quaternion.identity, transformComponent.model.rotation);
            Assert.AreEqual(Vector3.one, transformComponent.model.scale);
        }
    }
}
