using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityGLTF;
using Newtonsoft.Json;
using NUnit.Framework;
using DCL.Models;
using DCL.Components;
/*
* Play Mode Testing Highlights:
* - All Monobehaviour methods are invoked
* - Tests run in a standalone window
* - Tests may run slower, depending on the build target
*/

namespace Tests
{
    public class PlayModeTests
    {
        [UnityTest]
        public IEnumerator EntityCreation()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            Assert.AreNotEqual(null, sceneController);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.AreNotEqual(null, scene);

            // Create first entity
            string entityId = "1";

            TestHelpers.CreateSceneEntity(scene, entityId);
            var entityObject = scene.entities[entityId];

            Assert.IsTrue(entityObject != null);

            Assert.AreEqual(entityId, entityObject.entityId);

            // Create second entity
            entityObject = null;
            entityId = "2";

            TestHelpers.CreateSceneEntity(scene, entityId);
            scene.entities.TryGetValue(entityId, out entityObject);

            Assert.IsTrue(entityObject != null);

            Assert.AreEqual(entityId, entityObject.entityId);
        }

        [UnityTest]
        public IEnumerator EntityParenting()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            Assert.AreNotEqual(null, sceneController);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.AreNotEqual(null, scene);

            string entityId = "2";
            string parentEntityId = "3";

            TestHelpers.CreateSceneEntity(scene, entityId);
            TestHelpers.CreateSceneEntity(scene, parentEntityId);

            Assert.IsTrue(
              scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
              "parent is set to the scene root"
            );

            var parentEntityObject = scene.entities[parentEntityId];

            scene.SetEntityParent("{\"entityId\": \"" + entityId + "\"," + "\"parentId\": \"" + parentEntityId + "\"}");

            Assert.IsTrue(
              scene.entities[entityId].gameObject.transform.parent == parentEntityObject.gameObject.transform,
              "parent is set to parentId"
            );

            scene.SetEntityParent("{\"entityId\": \"" + entityId + "\"," + "\"parentId\": \"0\"}");

            Assert.IsTrue(
              scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
              "parent is set back to the scene root"
            );
        }

        // TODO: Find a way to run this test on Unity Cloud Build, even though it passes locally, it fails on timeout in Unity Cloud Build
        [UnityTest]
        [Explicit("This test fails in cloud build")]
        public IEnumerator CreateAnimationComponent()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForEndOfFrame();

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForEndOfFrame();

            string entityId = "1";
            scene.CreateEntity(JsonUtility.ToJson(new DCL.Models.CreateEntityMessage
            {
                id = entityId
            }));

            Assert.IsNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/CesiumMan/CesiumMan.glb"
            }));

            string animJson = JsonConvert.SerializeObject(new DCLAnimator.Model
            {
                states = new DCLAnimator.Model.DCLAnimationState[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "clip01",
                        clip = "animation:0",
                        playing = true,
                        weight = 1,
                        speed = 1
                    }
                }
            });

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "animation",
                classId = (int)DCL.Models.CLASS_ID.ANIMATOR,
                json = animJson
            }));


            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>();

            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<Animator>(), "'GLTFScene' child object with 'Animator' component should exist if the GLTF was loaded correctly.");
            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<DCLAnimator>(), "'GLTFScene' child object with 'DCLAnimator' component should exist if the GLTF was loaded correctly.");

            yield return new WaitForSeconds(0.5f);
            DCLAnimator dclAnimator = scene.entities[entityId].gameObject.GetComponentInChildren<DCLAnimator>();

            Assert.IsNotNull(dclAnimator.GetStateByString("clip01"), "dclAnimator.GetStateByString fail!");
            Assert.IsNotNull(dclAnimator.model.states[0].clipReference, "dclAnimator clipReference is null!");
        }

        [UnityTest]
        public IEnumerator TransformUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            Assert.AreNotEqual(null, sceneController);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.AreNotEqual(null, scene);

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
                    classId = CLASS_ID.TRANSFORM,
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
                    classId = (int)CLASS_ID.TRANSFORM,
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
        public IEnumerator BoxShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);
        }

        [UnityTest]
        public IEnumerator SphereShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "2";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.SPHERE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Sphere Instance", meshName);
        }

        [UnityTest]
        public IEnumerator PlaneShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "3";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Plane Instance", meshName);
        }

        [UnityTest]
        public IEnumerator CylinderShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "5";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CYLINDER_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
        }

        [UnityTest]
        public IEnumerator ConeShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "4";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CONE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cone50v0t1b2l2o Instance", meshName);
        }

        // TODO: Find a way to run this test on Unity Cloud Build, even though it passes locally, the webserver fails to find the .obj when running in unity cloud build...
        [UnityTest]
        [Explicit("This test fails in cloud build")]
        public IEnumerator OBJShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Material placeholderLoadingMaterial = Resources.Load<Material>("Materials/AssetLoading");

            yield return new WaitForSeconds(0.01f);

            Assert.IsNull(scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>(), "Since the shape hasn't been updated yet, the child renderer shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.OBJ_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/OBJ/teapot.obj"
            }));

            yield return new WaitForSeconds(8f);

            var childRenderer = scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>();
            Assert.IsNotNull(childRenderer, "Since the shape has already been updated, the child renderer should exist");
            Assert.AreNotSame(placeholderLoadingMaterial, childRenderer.sharedMaterial, "Since the shape has already been updated, the child renderer found shouldn't have the 'AssetLoading' placeholder material");
        }

        [UnityTest]
        public IEnumerator GLTFShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
        }

        [UnityTest]
        public IEnumerator ShapeMeshObjectIsReused()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // Set its shape as a BOX
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, "{}");

            yield return new WaitForSeconds(0.01f);

            var originalMeshGO = scene.entities[entityId].meshGameObject;

            Assert.IsTrue(originalMeshGO != null, "originalMeshGO is not null");

            // Update its shape to a SPHERE
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.SPHERE_SHAPE, "{}");

            var newMeshGO = scene.entities[entityId].meshGameObject;

            Assert.IsTrue(originalMeshGO != null, "originalMeshGO is not null");
            Assert.IsTrue(newMeshGO, "newMeshGO is not null");
            Assert.AreSame(newMeshGO, originalMeshGO, "meshGameObject must be reused across different shapes");
        }


        [UnityTest]
        public IEnumerator PreExistentShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            Assert.IsTrue(entity.meshGameObject == null, "meshGameObject should be null");
            // Set its shape as a BOX
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");
            yield return new WaitForSeconds(0.01f);

            var meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);

            // Update its shape to a cylinder
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");
            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(entity.meshGameObject != null, "meshGameObject should not be null");


            meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsNotNull(entity.meshGameObject.GetComponent<MeshFilter>(), "After updating the entity shape to a basic shape, the mesh filter shouldn't be removed from the object");

            Assert.IsTrue(entity.currentShape != null, "current shape must exist 1");
            Assert.IsTrue(entity.currentShape is CylinderShape, "current shape is BoxShape");

            // Update its shape to a GLTF
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsTrue(entity.currentShape != null, "current shape must exist 2");
            Assert.IsTrue(entity.currentShape is GLTFShape, "current shape is GLTFShape");

            Assert.IsNotNull(entity.meshGameObject);

            Assert.IsNull(entity.meshGameObject.GetComponent<MeshFilter>(), "After updating the entity shape to a GLTF shape, the mesh filter should be removed from the object");
            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            // Update its shape to a sphere
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");
            yield return new WaitForSeconds(1f);

            Assert.IsNotNull(entity.meshGameObject);

            meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "'GLTFScene' child object with 'InstantiatedGLTF' component shouldn't exist after the shape is updated to a non-GLTF shape");
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<GLTFComponent>().loadedAssetRootGameObject.transform.Find("Lantern"));

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/DamagedHelmet/DamagedHelmet.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<GLTFComponent>().loadedAssetRootGameObject.transform.Find("node_damagedHelmet_-6514"));
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeImmediateUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb"
            }));

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/DamagedHelmet/DamagedHelmet.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.AreEqual(1, scene.entities[entityId].gameObject.GetComponentsInChildren<InstantiatedGLTFObject>().Length, "Only 1 'InstantiatedGLTFObject' should remain once the GLTF shape has been updated");
        }

        [UnityTest]
        public IEnumerator ShapeWithCollisionsUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "transform",
                classId = (int)DCL.Models.CLASS_ID.TRANSFORM,
                json = JsonConvert.SerializeObject(new
                {
                    position = Vector3.zero,
                    scale = new Vector3(1, 1, 1),
                    rotation = new
                    {
                        x = 0,
                        y = 0,
                        z = 0,
                        w = 1
                    }
                })
            }));

            // Update shape without collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = false
            }));

            yield return new WaitForSeconds(0.01f);

            Assert.IsNull(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>());

            // Update shape with collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = true
            }));

            yield return new WaitForSeconds(0.01f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>());

            // Update shape without collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = false
            }));

            yield return new WaitForSeconds(0.01f);

            Assert.IsFalse(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>().enabled);

            // Update shape with collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = true
            }));

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>().enabled);
        }

        [UnityTest]
        public IEnumerator GLTFShapeWithCollisionsUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/Roads/Roads.glb" // this glb has the "..._collider" object inside with the pre-defined collision geometry.
            }));

            yield return new WaitForSeconds(8f);

            var colliderObject = scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>();
            Assert.IsNotNull(colliderObject);
            Assert.IsFalse(colliderObject.GetComponent<MeshRenderer>().enabled);
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithBasicShape()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

            yield return new WaitForSeconds(0.01f);

            string clickUuid = "click-1";
            TestHelpers.AddOnClickComponent(scene, entityId, clickUuid);

            yield return new WaitForSeconds(0.01f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponent<Rigidbody>(), "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onClickCollider = meshFilter.transform.Find("OnClickCollider");

            Assert.IsNotNull(onClickCollider, "OnClickCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShape()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";

            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "click-1";
            TestHelpers.AddOnClickComponent(scene, entityId, clickUuid);

            yield return new WaitForSeconds(0.01f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponent<Rigidbody>(), "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onClickCollider = meshFilter.transform.Find("OnClickCollider");

                Assert.IsNotNull(onClickCollider, "OnClickCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShapeAsynchronously()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb"
            }));

            string clickUuid = "click-1";
            TestHelpers.AddOnClickComponent(scene, entityId, clickUuid);

            yield return new WaitForSeconds(8f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>(), "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            yield return new WaitForSeconds(0.01f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponent<Rigidbody>(), "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onClickCollider = meshFilter.transform.Find("OnClickCollider");

                Assert.IsNotNull(onClickCollider, "OnClickCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesAfterBasicShapeIsAdded()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            string clickUuid = "click-1";
            TestHelpers.AddOnClickComponent(scene, entityId, clickUuid);

            yield return new WaitForSeconds(0.01f);

            Assert.IsNull(scene.entities[entityId].gameObject.GetComponent<Rigidbody>(), "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsNull(scene.entities[entityId].gameObject.transform.Find("OnClickCollider"), "the OnClickCollider object shouldn't exist until a shape is added");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
              JsonConvert.SerializeObject(new BoxModel { })
            );

            yield return new WaitForSeconds(0.01f);

            Assert.IsNotNull(scene.entities[entityId].gameObject.GetComponent<Rigidbody>(), "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onClickCollider = meshFilter.transform.Find("OnClickCollider");

            Assert.IsNotNull(onClickCollider, "OnClickCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator PBRMaterialUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            string materialID = "a-material";
            string textureURL = "http://127.0.0.1:9991/Images/atlas.png";

            TestHelpers.InstantiateEntityWithMaterial(scene, entityId, Vector3.zero, new DCL.Components.PBRMaterial.Model
            {
                albedoTexture = textureURL,
                metallic = 0,
                roughness = 1,
                hasAlpha = true
            }, materialID);

            var materialComponent = scene.disposableComponents[materialID] as DCL.Components.PBRMaterial;

            Assert.IsTrue(materialComponent is DCL.Components.PBRMaterial, "material is PBRMaterial");

            yield return materialComponent.routine;

            {
                var meshRenderer = scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>();
                Assert.IsNotNull(meshRenderer, "MeshRenderer must exist");
                var assignedMaterial = meshRenderer.sharedMaterial;
                Assert.IsNotNull(meshRenderer, "MeshRenderer.sharedMaterial must be the same as assignedMaterial");
                Assert.AreEqual(assignedMaterial, materialComponent.material, "Assigned material");
                Assert.AreEqual(textureURL, materialComponent.model.albedoTexture, "Texture data must be correct");
                var loadedTexture = meshRenderer.sharedMaterial.mainTexture;
                Assert.IsNotNull(loadedTexture, "Texture must be loaded");
            }
        }

        [UnityTest]
        public IEnumerator PBRMaterialPropertiesUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            string materialID = "a-material";

            // Instantiate entity with default PBR Material
            TestHelpers.InstantiateEntityWithMaterial(scene, entityId, Vector3.zero, new DCL.Components.PBRMaterial.Model(), materialID);

            var materialComponent = scene.disposableComponents[materialID] as DCL.Components.PBRMaterial;

            yield return materialComponent.routine;

            Assert.IsTrue(materialComponent is DCL.Components.PBRMaterial, "material is PBRMaterial");

            // Check if material initialized correctly
            {
                var meshRenderer = scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>();

                Assert.IsNotNull(meshRenderer, "MeshRenderer must exist");

                var assignedMaterial = meshRenderer.sharedMaterial;
                Assert.IsNotNull(meshRenderer, "MeshRenderer.sharedMaterial must be the same as assignedMaterial");

                Assert.AreEqual(assignedMaterial, materialComponent.material, "Assigned material");
            }

            // Check default properties
            {
                // Texture
                Assert.IsNull(materialComponent.material.GetTexture("_MainTex"));

                // Colors
                Assert.AreEqual("FFFFFF", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_Color")));
                Assert.AreEqual("000000", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_EmissionColor")));
                Assert.AreEqual("FFFFFF", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_SpecColor")));

                // Other properties
                Assert.AreEqual("0.5", materialComponent.material.GetFloat("_Metallic").ToString());
                Assert.AreEqual("0.5", materialComponent.material.GetFloat("_Glossiness").ToString());
                Assert.AreEqual("1", materialComponent.material.GetFloat("_GlossyReflections").ToString());
                Assert.AreEqual("1", materialComponent.material.GetFloat("_SpecularHighlights").ToString());
                Assert.AreEqual("1", materialComponent.material.GetFloat("_AlphaClip").ToString());
                Assert.AreEqual(2000, materialComponent.material.renderQueue);
            }

            // Update material
            string textureURL = "http://127.0.0.1:9991/Images/atlas.png";

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = materialID,
                json = JsonUtility.ToJson(new DCL.Components.PBRMaterial.Model
                {
                    albedoTexture = textureURL,
                    albedoColor = "#99deff",
                    emissiveColor = "#42f4aa",
                    reflectivityColor = "#601121",
                    metallic = 0.37f,
                    roughness = 0.9f,
                    microSurface = 0.4f,
                    specularIntensity = 2f,
                    alpha = 0.5f,
                    transparencyMode = 2,
                    hasAlpha = true
                })
            }));

            yield return materialComponent.routine;

            // Check updated properties
            {
                // Texture
                Assert.IsNotNull(materialComponent.material.GetTexture("_MainTex"));

                // Colors
                Assert.AreEqual("99DEFF", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_Color")));
                Assert.AreEqual("42F4AA", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_EmissionColor")));
                Assert.AreEqual("601121", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_SpecColor")));

                // Other properties
                Assert.AreEqual("0.37", materialComponent.material.GetFloat("_Metallic").ToString());
                Assert.AreEqual("0.1", materialComponent.material.GetFloat("_Glossiness").ToString());
                Assert.AreEqual("0.4", materialComponent.material.GetFloat("_GlossyReflections").ToString());
                Assert.AreEqual("2", materialComponent.material.GetFloat("_SpecularHighlights").ToString());
                Assert.AreEqual("0.5", materialComponent.material.GetFloat("_AlphaClip").ToString());
                Assert.AreEqual(3000, materialComponent.material.renderQueue);
                Assert.AreEqual((int)UnityEngine.Rendering.BlendMode.SrcAlpha, materialComponent.material.GetInt("_SrcBlend"));
                Assert.AreEqual((int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, materialComponent.material.GetInt("_DstBlend"));
                Assert.AreEqual(0, materialComponent.material.GetInt("_ZWrite"));
            }
        }

        [UnityTest]
        public IEnumerator MaterialIsSharedCorrectly()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            // Create first entity with material
            string firstEntityID = "1";
            string firstMaterialID = "a-material";

            TestHelpers.InstantiateEntityWithMaterial(scene, firstEntityID, Vector3.zero, new DCL.Components.PBRMaterial.Model
            {
                metallic = 0.3f,
            }, firstMaterialID);

            // Create second entity with material
            string secondEntityID = "2";
            string secondMaterialID = "b-material";

            TestHelpers.InstantiateEntityWithMaterial(scene, secondEntityID, Vector3.zero, new DCL.Components.PBRMaterial.Model
            {
                metallic = 0.66f,
            }, secondMaterialID);

            // Create third entity and assign 1st material
            string thirdEntityID = "3";

            TestHelpers.InstantiateEntityWithShape(scene, thirdEntityID, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);
            scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
            {
                entityId = thirdEntityID,
                id = firstMaterialID,
                name = "material"
            }));

            // Check renderers material references
            var firstRenderer = scene.entities[firstEntityID].gameObject.GetComponentInChildren<MeshRenderer>();
            var secondRenderer = scene.entities[secondEntityID].gameObject.GetComponentInChildren<MeshRenderer>();
            var thirdRenderer = scene.entities[thirdEntityID].gameObject.GetComponentInChildren<MeshRenderer>();
            Assert.AreNotSame(firstRenderer.sharedMaterial, secondRenderer.sharedMaterial, "1st and 2nd entities should have different materials");
            Assert.AreSame(firstRenderer.sharedMaterial, thirdRenderer.sharedMaterial, "1st and 3rd entities should have the same material");
        }

        [UnityTest]
        public IEnumerator MaterialUpdateAffectsCorrectEntities()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            // Create first entity with material
            string firstEntityID = "1";
            string firstMaterialID = "a-material";

            TestHelpers.InstantiateEntityWithMaterial(scene, firstEntityID, Vector3.zero, new DCL.Components.PBRMaterial.Model
            {
                metallic = 0.3f,
            }, firstMaterialID);

            // Create second entity with material
            string secondEntityID = "2";
            string secondMaterialID = "b-material";

            TestHelpers.InstantiateEntityWithMaterial(scene, secondEntityID, Vector3.zero, new DCL.Components.PBRMaterial.Model
            {
                metallic = 0.66f,
            }, secondMaterialID);

            // Create third entity and assign 1st material
            string thirdEntityID = "3";

            TestHelpers.InstantiateEntityWithShape(scene, thirdEntityID, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);
            scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
            {
                entityId = thirdEntityID,
                id = firstMaterialID,
                name = "material"
            }));

            // Check renderers material references
            var firstRenderer = scene.entities[firstEntityID].gameObject.GetComponentInChildren<MeshRenderer>();
            var secondRenderer = scene.entities[secondEntityID].gameObject.GetComponentInChildren<MeshRenderer>();
            var thirdRenderer = scene.entities[thirdEntityID].gameObject.GetComponentInChildren<MeshRenderer>();
            Assert.AreNotSame(firstRenderer.sharedMaterial, secondRenderer.sharedMaterial, "1st and 2nd entities should have different materials");
            Assert.AreSame(firstRenderer.sharedMaterial, thirdRenderer.sharedMaterial, "1st and 3rd entities should have the same material");

            // Check material properties before updating them
            Assert.AreEqual("0.3", firstRenderer.sharedMaterial.GetFloat("_Metallic").ToString());
            Assert.AreEqual("0.66", secondRenderer.sharedMaterial.GetFloat("_Metallic").ToString());

            // Update material properties
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = firstMaterialID,
                json = JsonUtility.ToJson(new DCL.Components.PBRMaterial.Model
                {
                    metallic = 0.95f
                })
            }));

            yield return (scene.disposableComponents[firstMaterialID] as DCL.Components.PBRMaterial).routine;

            // Check material properties after updating them
            Assert.AreEqual("0.95", firstRenderer.sharedMaterial.GetFloat("_Metallic").ToString());
            Assert.AreEqual("0.66", secondRenderer.sharedMaterial.GetFloat("_Metallic").ToString());
        }

        [UnityTest]
        public IEnumerator MaterialDetach()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            string materialID = "a-material";

            TestHelpers.InstantiateEntityWithMaterial(scene, entityId, Vector3.zero, new DCL.Components.BasicMaterial.Model(), materialID);

            var meshRenderer = scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>();
            var materialComponent = scene.disposableComponents[materialID] as DCL.Components.BasicMaterial;

            yield return materialComponent.routine;

            Assert.IsTrue(materialComponent is DCL.Components.BasicMaterial, "material is BasicMaterial");

            // Check if material initialized correctly
            {
                Assert.IsNotNull(meshRenderer, "MeshRenderer must exist");

                Assert.AreEqual(meshRenderer.sharedMaterial, materialComponent.material, "Assigned material");
            }

            // Remove material
            materialComponent.DetachFrom(scene.entities[entityId]);

            // Check if material was removed correctly
            Assert.IsNull(meshRenderer.sharedMaterial, "Assigned material should be null as it has been removed");
        }

        [UnityTest]
        public IEnumerator MaterialDisposedGetsDetached()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string firstEntityId = "1";
            string secondEntityId = "2";
            string materialID = "a-material";

            // Instantiate entity with material
            TestHelpers.InstantiateEntityWithMaterial(scene, firstEntityId, Vector3.zero, new DCL.Components.BasicMaterial.Model(), materialID);

            // Create 2nd entity and attach same material to it
            TestHelpers.InstantiateEntityWithShape(scene, secondEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);
            scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
            {
                entityId = secondEntityId,
                id = materialID,
                name = "material"
            }));

            var firstMeshRenderer = scene.entities[firstEntityId].gameObject.GetComponentInChildren<MeshRenderer>();
            var secondMeshRenderer = scene.entities[secondEntityId].gameObject.GetComponentInChildren<MeshRenderer>();
            var materialComponent = scene.disposableComponents[materialID] as DCL.Components.BasicMaterial;

            yield return materialComponent.routine;

            Assert.IsTrue(materialComponent is DCL.Components.BasicMaterial, "material is BasicMaterial");

            // Check if material attached correctly
            {
                Assert.IsNotNull(firstMeshRenderer, "MeshRenderer must exist");
                Assert.AreEqual(firstMeshRenderer.sharedMaterial, materialComponent.material, "Assigned material");

                Assert.IsNotNull(secondMeshRenderer, "MeshRenderer must exist");
                Assert.AreEqual(secondMeshRenderer.sharedMaterial, materialComponent.material, "Assigned material");
            }

            // Dispose material
            scene.SharedComponentDispose(JsonUtility.ToJson(new DCL.Models.SharedComponentDisposeMessage
            {
                id = materialID
            }));

            // Check if material detached correctly
            Assert.IsNull(firstMeshRenderer.sharedMaterial, "MeshRenderer must exist");
            Assert.IsNull(secondMeshRenderer.sharedMaterial, "MeshRenderer must exist");
        }

        [UnityTest]
        public IEnumerator EntityBasicMaterialUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            string materialID = "a-material";

            // Instantiate entity with default PBR Material
            TestHelpers.InstantiateEntityWithMaterial(scene, entityId, Vector3.zero, new DCL.Components.BasicMaterial.Model(), materialID);

            var meshRenderer = scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>();
            var materialComponent = scene.disposableComponents[materialID] as DCL.Components.BasicMaterial;

            yield return materialComponent.routine;

            Assert.IsTrue(materialComponent is DCL.Components.BasicMaterial, "material is BasicMaterial");

            // Check if material initialized correctly
            {
                Assert.IsNotNull(meshRenderer, "MeshRenderer must exist");

                var assignedMaterial = meshRenderer.sharedMaterial;
                Assert.IsNotNull(meshRenderer, "MeshRenderer.sharedMaterial must be the same as assignedMaterial");

                Assert.AreEqual(assignedMaterial, materialComponent.material, "Assigned material");
            }

            // Check default properties
            {
                Assert.IsNull(materialComponent.material.GetTexture("_MainTex"));
                Assert.AreEqual("1", materialComponent.material.GetFloat("_AlphaClip").ToString());
            }

            // Update material
            string textureURL = "http://127.0.0.1:9991/Images/atlas.png";

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = materialID,
                json = JsonUtility.ToJson(new DCL.Components.BasicMaterial.Model
                {
                    texture = textureURL,
                    samplingMode = 2,
                    wrap = 3,
                    alphaTest = 0.5f
                })
            }));

            yield return materialComponent.routine;

            // Check updated properties
            {
                Assert.IsNotNull(materialComponent.material.GetTexture("_MainTex"));
                Assert.AreEqual("0.5", materialComponent.material.GetFloat("_AlphaClip").ToString());
                Assert.AreEqual(TextureWrapMode.Mirror, materialComponent.material.mainTexture.wrapMode);
                Assert.AreEqual(FilterMode.Bilinear, materialComponent.material.mainTexture.filterMode);
            }
        }

        [UnityTest]
        public IEnumerator SceneLoading()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

            string loadedSceneID = "0,0";

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

            Assert.IsTrue(sceneController.loadedScenes[loadedSceneID] != null);
        }

        [UnityTest]
        public IEnumerator EntityRemoval()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            string entityId = "2";

            Assert.AreNotEqual(null, sceneController);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.AreNotEqual(null, scene);

            TestHelpers.CreateSceneEntity(scene, entityId);

            var gameObjectReference = scene.entities[entityId].gameObject;

            TestHelpers.RemoveSceneEntity(scene, entityId);

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(!scene.entities.ContainsKey(entityId));

            Assert.IsTrue(gameObjectReference == null, "Entity gameobject reference is not getting destroyed.");
        }

        [UnityTest]
        public IEnumerator SceneUnloading()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

            string loadedSceneID = "0,0";

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

            // Add 1 entity to the loaded scene
            TestHelpers.CreateSceneEntity(sceneController.loadedScenes[loadedSceneID], "6");

            var sceneRootGameObject = sceneController.loadedScenes[loadedSceneID];
            var sceneEntities = sceneController.loadedScenes[loadedSceneID].entities;

            sceneController.UnloadScene(loadedSceneID);

            yield return new WaitForSeconds(0.01f); // We wait to let unity destroy gameobjects.

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID) == false);

            Assert.IsTrue(sceneRootGameObject == null, "Scene root gameobject reference is not getting destroyed.");

            Assert.AreEqual(sceneEntities.Count, 0, "Every entity should be removed");
        }

        [UnityTest]
        public IEnumerator SeveralParcelsFromJSON()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var jsonMessageToLoad = "{\"parcelsToLoad\":[{\"id\":\"-102,100\",\"basePosition\":{\"x\":-102,\"y\":100},\"parcels\":[{\"x\":-102,\"y\":100}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm39j2mCramRURmogUMqtrXNHtZ3z8taShBRLinPZGWmjFzKnL2A1JTortYo77aUYETPmRoHJdn2qhYqWk3acKoqnW\"},{\"file\":\"game.ts\",\"hash\":\"Qm318JHUVnzsZKA8fUeuSvcz6VepGg3oJVryiUwRb3QnDJq4HL5HFRKZKocUtDB96HUwWZReRXPeR9KiLc9q1h2f5Y\"},{\"file\":\"scene.json\",\"hash\":\"Qm4DSDqaibeuwWkXNX6BsafcMU2BhUU64V2MoUxtrqDMpzXFwEUQ1FPLdHNysYCnPhVVpQ6XgTvxCfz4KikaPwkmq1\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm2ko8HNC9m8yHfqsuH76wsZYhhJajEYRVFYkypCsJQKeM18MyN5E3PfE2pLqARqnxD3UNApvg1XYnYFxw8Rkdouhm\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,100\",\"basePosition\":{\"x\":-100,\"y\":100},\"parcels\":[{\"x\":-100,\"y\":100}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm5S3QXrcnq8ScRCw1cwWe3SAWr5c5Mh5eEFgRnayVxp26HwS2mBqEpHMmSP45czoDn6UQ4zT3sETy7ABFU4Q4yiiF\"},{\"file\":\"game.ts\",\"hash\":\"Qm4XSDU2cnWFDFrK74SjmCuysW8SUScozPvbwomXL2AKaNbYC8P33UMqr8EXAxzhdT4bFCXJ1K8z74hMp5RxbcLznV\"},{\"file\":\"scene.json\",\"hash\":\"Qm51UBhp1EsViXFLyX8cpVLqYrafJoVdCr7Ux1j7dB9u7DwCV9gsLZSmjLewHTCkMn2AH7ttnNwYnv5u2g7Q94RNtc\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm5uXQmcazfvBDw1cwortCTRXvhVz4Tx9SzfjQV5NpEbxnaRCtgoDe3cxvwbZBdujZpUkATmmYCAfH1Q69P4oPS2Z6\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,101\",\"basePosition\":{\"x\":-100,\"y\":101},\"parcels\":[{\"x\":-100,\"y\":101}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm48ZepPVVTRFNZXU6Cuu11xSNLc4AWWDf4cXb7zucRMm2VMAWmm8JdXLWfTwJP2T1K1NAWkJJjX185TYbzdqEXUiQ\"},{\"file\":\"game.ts\",\"hash\":\"QmjhK1mT7Q5QRHVtTbMScVdwTSDBXxUuYeyhdHbTYwKMrDLqg8JM9S7vTFUmaaP5FqTVaEruyrHiBrJDcZ9T87gfx\"},{\"file\":\"scene.json\",\"hash\":\"Qm2wa4G47XM6c4zvk2j4hdfSgVPmmS1rnLA1xiJ13kZ4VHBYsBieHkPQ2Eh5FV8hZcUddoMJkbkAvMJA7v8AggsFcN\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm3m45w9a87xaBQk2RCk23F81zjmeHPJ5k11sYnRKzmYUFqbt7Y3yA1ARshwyuTjEVq4x8z6QfJCuEDA2aQH994V7X\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,102\",\"basePosition\":{\"x\":-100,\"y\":102},\"parcels\":[{\"x\":-100,\"y\":102}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"QmgnZBWtSZW76ViogXckDG4qVbUei7tSnCZBp8FdybYFs8GvqjC5vBNg3Bk4KwksprrD9HMVyHqX96kc5ZCwkU45d\"},{\"file\":\"game.ts\",\"hash\":\"Qm4QJQqWvFf3QVMFNdcTWS84CKLCiL2mFbPfp1GJq7NkwKdgY59f5MsLwyhw3xntgQvWbourEuxhJ42qBuAnsWYQmB\"},{\"file\":\"scene.json\",\"hash\":\"Qm2NRxf8pDoMEiKSeNCBGE57F8RCKWYuB8Jin3RpMgEa4w5YCasQfw4ReGPkKbgSFK99XX8GvzSegoqXWjAUnnRKyq\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm8yAaa84utvn1JiemEqiRUf5TmqHKtFBr5AJFcgyhCxrN36zEBWxqtQbHp7S2GRKMdo8sZvi3xvxbgoBnMtkJHfD\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,103\",\"basePosition\":{\"x\":-100,\"y\":103},\"parcels\":[{\"x\":-100,\"y\":103}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm2Mxae9qDm3zxxaj9TiRGYK29mPGKM5ffrBgvfAsarCs9VXLTRRERDibn165TtS7emP3zJ16Co9rYLTYQEety6fyb\"},{\"file\":\"game.ts\",\"hash\":\"Qm5xR3gEV9qppTqUwro1p9oWffMFopyZ1ejnN8rnvQ3gArLh8QMZtezNp82wtE4K4rzvGYpHS4Z3DoonnR19YmzB2z\"},{\"file\":\"scene.json\",\"hash\":\"Qm2vCKt6Vc3eBkHSZ8xjLwpGVwNfqxRDghcsaknMCtKrJE3jeA3KEFqkK9HRK5RDaeH6N8vosd1f7hf3VxBQuZCu5L\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm3JnfFM9xzeeaj2LVAr65sCmUupRcMoHkS4XEgKrwhcmDipzqkLSZKyvpQjxd8F7vu92MMsnqbna23KWcTDaNqGak\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,104\",\"basePosition\":{\"x\":-100,\"y\":104},\"parcels\":[{\"x\":-100,\"y\":104}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"QmwS5L1VbRn7agFYf5W8wx75SrKL4ayF3bYKxMrGq9M4sipT2isc8souk3WBLy2SvBgSU7RyktSLVF7eSVzeR1atC\"},{\"file\":\"game.ts\",\"hash\":\"Qm3KU6Ne5UXCDR4kfuchoRXBJNh85WHn25eX7STRGpfRz56iFza6sPioGc7C9HHEPBDWNJGgPh4nf3zt7XHsXAWAez\"},{\"file\":\"scene.json\",\"hash\":\"Qm5tamJRukS9EqfGhTgCvF1Dz9hFKKh7WT1kFzLZbJpwETBe9Ak8qMfFvXZSyQ7mtQN3BPUKWXp3FRa1G8XDjmYhor\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm4CCi8esaBNe6JTGhPFafSFZMdHyhyEwpniLvpA5ENbBJ3skyT9bQZtXYV9VwGSEkcbRag9ngp4dzaf1ENVVarM72\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,105\",\"basePosition\":{\"x\":-100,\"y\":105},\"parcels\":[{\"x\":-100,\"y\":105}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"atlas.png\",\"hash\":\"Qm3HiKF3yPmxg6kGtBZmNeDxL5GhHwaAqr3eeg49k2EWmh1geRWjVTFjLBCptun2q4uKdohqy2BwMpk6KNBQRTQNcT\"},{\"file\":\"game.js\",\"hash\":\"Qm5M4pYTXht64o156etmchHkxgkQevoFyE6NbLGQzoPS25RfCbsUykXU419k2m81Axkn96yS4Qipqz72cUuhjJKvva\"},{\"file\":\"game.ts\",\"hash\":\"Qm4wDCXm9hhQzJD3FJm1dNdgmK8W82jgEcxCKFLv48NyNBMpsx28PTJjzej6ZrUTy87RT3TVytJKKBv8D73FmFvKcw\"},{\"file\":\"scene.json\",\"hash\":\"Qm2TC7GgDnxKPnixHN6YxjGM6VV3pkHxKKC1Cqzvjzb5kcUWriHD6xpkNVPuef7EntUCt8XvWSYGaV2DwrsHP7cAHF\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm53AtLjaoYbsZvcnwgDv3erd85pnL8zR7WbrQxEnPusfkZ4ysCaLhqBbn99EvdvAvN5U6ZfzJjYdQRjefwaRzsHjd\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,106\",\"basePosition\":{\"x\":-100,\"y\":106},\"parcels\":[{\"x\":-100,\"y\":106}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm4c5CSSu37s55Hd21jRCrBSn1x6bdPT3UGQyshtR9WAwGftGgrsTAK6uEd4H6RV2DdZtRyojQzjKQMyonMCrPLxop\"},{\"file\":\"game.ts\",\"hash\":\"Qm3qB6u32Tzqd1K7Rfu4ZATzgjTtrxPu6FBENiHUNZUrj66wochUYFpoXAqXUXeufnPZTn12RZsT2nbDANXWFevqJg\"},{\"file\":\"scene.json\",\"hash\":\"Qm4MpHJooeFESeQfMZaHiVoE1NhLHpYvSUctnHjUn3L9qR9V4nA9wg3bcXQPoLkXuR31Q9uJDVGnj7MPtmzgyyMBMk\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm4mXeoioxNryxn1741bHi16JtLsVYB6PLrbo7T6LAd9ZYfK4bupRyNRioxvr6fUXnHNDy7W2HHysPrqn4nVxEgHkT\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,107\",\"basePosition\":{\"x\":-100,\"y\":107},\"parcels\":[{\"x\":-100,\"y\":107}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm2FcLJwy5fGZjw7bss2HQiXAB1TGZpoQDhGDFfwBfgwtq9E98VgwZzYgubfvdqoppp61T1ydkZXZvDbV3j2eXuMa4\"},{\"file\":\"game.ts\",\"hash\":\"Qm4urxWNwqg115gcE3QCLZUNMMTNeSpfpVcXhW73dr1cjwySUMuwbduLL7QH94KA84iG3cy7SBBwMHSgmZ8URzj97h\"},{\"file\":\"scene.json\",\"hash\":\"Qm382D2nHx8swSYXaHPtETjpEwkqx35PRF1pMXZktLSHJ5tCTzEUZYYTVMbmzNKZH8ZF6CX9mpe5THBAhj4wcRzxuv\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm4v2uPzypAHRP6UPb19Ed12pLi8hM6hq2kbyndYU386sKHPRTZzs5g2AjTV6CPDNqakS8hnbhZRhzTkZyZQE98NQH\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,108\",\"basePosition\":{\"x\":-100,\"y\":108},\"parcels\":[{\"x\":-100,\"y\":108}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm4x4T311KqBj67T8wgSKiC5QndfdySRaTLgcSHDKE5Ccqpu7xmB6KYcuWJAUvQYaWcgHc8UTqbJ9g8BhWDmemzR1\"},{\"file\":\"game.ts\",\"hash\":\"Qm2PJ6somc5ijqzjcVe7vxERKpkPHyYEUUFysGrkzCXL6BjabDGVb2fzxgC5nS5hDApSHGBYg7HYG4W4wnhLmxmfDY\"},{\"file\":\"scene.json\",\"hash\":\"Qm51RXkydt57AeYgs9p8B1daURhH5AmbTFc9NFJA5X1eo3goE8qzwTcCHDuc63hi8C9VAU3zaL1NBjj3XAZnPNMbAC\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm2tWYaRPqP9Wh7e7EKdWbjuX34gUPh57gQAE8yCK3jbmMxabJVZybVwiAXiQUfFMgpYjxGWCZ8YEQiBJs8ZzrUdjz\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,109\",\"basePosition\":{\"x\":-100,\"y\":109},\"parcels\":[{\"x\":-100,\"y\":109}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"bubble.png\",\"hash\":\"Qm2XNpLngcqqH4Wgy6jDqL3SrthNdUGiAftrdctMZkvvJLT7s1JT4ffGp4oWWRDiGf8vAb5FxSXMjidT6HmbinoY91\"},{\"file\":\"game.js\",\"hash\":\"Qm3YBdwfioGe1Zbd69VnqnDTm497crH6b5fpe7J4gTS69KR53gUdSxnNar5C89F9qN8PnefJZDyujFvHgcF4jACY1X\"},{\"file\":\"game.ts\",\"hash\":\"Qm48ngfKEEtDPfrxSVkdCtNEApNMNTnUPJ8WeFLvdGaGBgFXg9HQnETnz8Z4r8E8th5GREUobBjL164YBK14NHNnFM\"},{\"file\":\"scene.json\",\"hash\":\"Qm5vb5xVA4JENeFXBCJg7npTkYm1eG4gfpHj2GARdMgrsAtcuHqA3PTA3UpWeND9Em7Emgx8YmzbLfaiqDyiXNpS8X\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm2Rqm29ZNLdFD9Zak72w2k7fUL8dp6G6TNYFLFpCBaNga9dBd4MNfbeVA4EooxZN1bqkt6eXTT8HwJK6XmAsqToyZ\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}]}";

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            var referenceCheck = new System.Collections.Generic.List<DCL.Controllers.ParcelScene>();

            foreach (var kvp in sceneController.loadedScenes)
            {
                referenceCheck.Add(kvp.Value);
            };

            Assert.AreEqual(sceneController.loadedScenes.Count, 11);

            sceneController.LoadParcelScenes(jsonMessageToLoad);

            Assert.AreEqual(sceneController.loadedScenes.Count, 11);

            foreach (var reference in referenceCheck)
            {
                Assert.IsTrue(sceneController.loadedScenes.ContainsValue(reference), "References must be the same");
            };
        }

        [UnityTest]
        public IEnumerator PositionParcels()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var jsonMessageToLoad = "{\"parcelsToLoad\":[{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}]}";

            DCL.Configuration.Environment.DEBUG = true;

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            sceneController.LoadParcelScenes(jsonMessageToLoad);
            Assert.AreEqual(sceneController.loadedScenes.Count, 1);

            var theScene = sceneController.loadedScenes["xxx"];

            Assert.AreEqual(theScene.sceneData.parcels.Length, 3);
            Assert.AreEqual(theScene.transform.childCount, 3);

            Assert.IsTrue(theScene.transform.GetChild(0).localPosition == new Vector3(-10.0f + 5f, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, 0.0f + 5f));
            Assert.IsTrue(theScene.transform.GetChild(1).localPosition == new Vector3(0.0f + 5f, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, 0.0f + 5f));
            Assert.IsTrue(theScene.transform.GetChild(2).localPosition == new Vector3(-10.0f + 5f, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, 10.0f + 5f));
        }

        [UnityTest]
        public IEnumerator PositionParcels2()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var jsonMessageToLoad = "{\"parcelsToLoad\":[{\"id\":\"xxx\",\"basePosition\":{\"x\":90,\"y\":90},\"parcels\":[{\"x\":89,\"y\":90}, {\"x\":90,\"y\":90}, {\"x\":89,\"y\":91}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}]}";

            DCL.Configuration.Environment.DEBUG = true;

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            sceneController.LoadParcelScenes(jsonMessageToLoad);
            Assert.AreEqual(sceneController.loadedScenes.Count, 1);

            var theScene = sceneController.loadedScenes["xxx"];

            Assert.AreEqual(theScene.sceneData.parcels.Length, 3);
            Assert.AreEqual(theScene.transform.childCount, 3);

            Assert.IsTrue(theScene.transform.GetChild(0).localPosition == new Vector3(-10.0f + 5f, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, 0.0f + 5f));
            Assert.IsTrue(theScene.transform.GetChild(1).localPosition == new Vector3(0.0f + 5f, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, 0.0f + 5f));
            Assert.IsTrue(theScene.transform.GetChild(2).localPosition == new Vector3(-10.0f + 5f, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, 10.0f + 5f));
        }
    }
}
