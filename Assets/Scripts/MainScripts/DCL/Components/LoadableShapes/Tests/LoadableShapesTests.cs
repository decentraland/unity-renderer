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

namespace Tests
{
    public class LoadableShapesTests
    {
        [UnityTest]
        public IEnumerator OBJShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Material placeholderLoadingMaterial = Resources.Load<Material>("Materials/AssetLoading");

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(scene.entities[entityId].meshGameObject == null, "Since the shape hasn't been updated yet, the child mesh shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.OBJ_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/OBJ/teapot.obj"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsTrue(scene.entities[entityId].meshGameObject != null, "Every entity with a shape should have the mandatory 'Mesh' object as a child");

            var childRenderer = scene.entities[entityId].meshGameObject.GetComponentInChildren<MeshRenderer>();
            Assert.IsTrue(childRenderer != null, "Since the shape has already been updated, the child renderer should exist");
            Assert.AreNotSame(placeholderLoadingMaterial, childRenderer.sharedMaterial, "Since the shape has already been updated, the child renderer found shouldn't have the 'AssetLoading' placeholder material");
        }

        [UnityTest]
        public IEnumerator GLTFShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null, "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
        }

        [UnityTest]
        public IEnumerator ShapeMeshObjectIsReused()
        {
            var sceneController = TestHelpers.InitializeSceneController();

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
            Assert.AreNotSame(newMeshGO, originalMeshGO, "meshGameObject must NOT be reused across different shapes, because Destroy() delays a frame and the Detach/Attach behaviour wouldn't work. We shouldn't use DestroyImmediate in runtime.");
        }


        [UnityTest]
        public IEnumerator PreExistentShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

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
            yield return new WaitForSeconds(8.01f);

            Assert.IsTrue(entity.meshGameObject != null, "meshGameObject should not be null");


            meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsTrue(entity.meshGameObject.GetComponent<MeshFilter>() != null, "After updating the entity shape to a basic shape, the mesh filter shouldn't be removed from the object");

            Assert.IsTrue(entity.currentShape != null, "current shape must exist 1");
            Assert.IsTrue(entity.currentShape is CylinderShape, "current shape is BoxShape");

            // Update its shape to a GLTF
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsTrue(entity.currentShape != null, "current shape must exist 2");
            Assert.IsTrue(entity.currentShape is GLTFShape, "current shape is GLTFShape");

            Assert.IsTrue(entity.meshGameObject != null);

            Assert.IsTrue(entity.meshGameObject.GetComponent<MeshFilter>() == null, "After updating the entity shape to a GLTF shape, the mesh filter should be removed from the object");
            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null, "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            // Update its shape to a sphere
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(entity.meshGameObject != null);

            meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "'GLTFScene' child object with 'InstantiatedGLTF' component shouldn't exist after the shape is updated to a non-GLTF shape");
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<GLTFComponent>().loadedAssetRootGameObject.transform.Find("Lantern") != null);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
            }));

            yield return new WaitForSeconds(8f);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<GLTFComponent>().loadedAssetRootGameObject.transform.Find("node_damagedHelmet_-6514") != null);
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeImmediateUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
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
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TRANSFORM,
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

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>() == null);

            // Update shape with collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = true
            }));

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>() != null);

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
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb" // this glb has the "..._collider" object inside with the pre-defined collision geometry.
            }));

            yield return new WaitForSeconds(8f);

            var colliderObject = scene.entities[entityId].gameObject.GetComponentInChildren<Collider>();
            Assert.IsTrue(colliderObject != null);
            Assert.IsTrue(colliderObject.GetComponent<MeshRenderer>() == null);
        }
    }
}
