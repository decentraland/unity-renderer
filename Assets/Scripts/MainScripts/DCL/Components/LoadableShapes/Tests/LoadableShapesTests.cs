using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Components;
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
    public class LoadableShapesTests : TestsBase
    {
        [UnityTest]
        public IEnumerator OBJShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Material placeholderLoadingMaterial = Resources.Load<Material>("Materials/AssetLoading");

            yield return null;

            Assert.IsTrue(scene.entities[entityId].meshGameObject == null, "Since the shape hasn't been updated yet, the child mesh shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.OBJ_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/OBJ/teapot.obj"
            }));

            OBJLoader objShape = scene.entities[entityId].gameObject.GetComponentInChildren<OBJLoader>(true);
            yield return new WaitUntil(() => objShape.alreadyLoaded);

            Assert.IsTrue(scene.entities[entityId].meshGameObject != null, "Every entity with a shape should have the mandatory 'Mesh' object as a child");

            var childRenderer = scene.entities[entityId].meshGameObject.GetComponentInChildren<MeshRenderer>();
            Assert.IsTrue(childRenderer != null, "Since the shape has already been updated, the child renderer should exist");
            Assert.AreNotSame(placeholderLoadingMaterial, childRenderer.sharedMaterial, "Since the shape has already been updated, the child renderer found shouldn't have the 'AssetLoading' placeholder material");
        }

        [UnityTest]
        public IEnumerator GLTFShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null, "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
        }

        [UnityTest]
        public IEnumerator PreExistentShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            Assert.IsTrue(entity.meshGameObject == null, "meshGameObject should be null");

            // Set its shape as a BOX
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");
            yield return null;

            var meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);

            // Update its shape to a cylinder
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");
            yield return null;

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

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(entity.currentShape != null, "current shape must exist 2");
            Assert.IsTrue(entity.currentShape is GLTFShape, "current shape is GLTFShape");

            Assert.IsTrue(entity.meshGameObject != null);

            Assert.IsTrue(entity.meshGameObject.GetComponent<MeshFilter>() == null, "After updating the entity shape to a GLTF shape, the mesh filter should be removed from the object");
            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null, "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            // Update its shape to a sphere
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");

            yield return null;

            Assert.IsTrue(entity.meshGameObject != null);

            meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "'GLTFScene' child object with 'InstantiatedGLTF' component shouldn't exist after the shape is updated to a non-GLTF shape");
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            {
                var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

                Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in first object!");
                Assert.IsTrue(gltfObject.transform.Find("Lantern") != null, "Can't find \"Lantern!\"");
            }

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
            }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            {
                var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

                Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in second object!");
                Assert.IsTrue(gltfObject.transform.Find("node_damagedHelmet_-6514") != null, "Can't find \"node_damagedHelmet_-6514\"!");
            }
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeImmediateUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
            }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.AreEqual(1, scene.entities[entityId].gameObject.GetComponentsInChildren<InstantiatedGLTFObject>().Length, "Only 1 'InstantiatedGLTFObject' should remain once the GLTF shape has been updated");
        }

        [UnityTest]
        public IEnumerator ShapeWithCollisionsUpdate()
        {
            yield return InitScene();

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

            yield return null;

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>() == null);

            // Update shape with collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = true
            }));

            yield return null;

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>() != null);

            // Update shape without collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = false
            }));

            yield return null;

            Assert.IsFalse(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>().enabled);

            // Update shape with collision
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, JsonConvert.SerializeObject(new
            {
                withCollisions = true
            }));

            yield return null;

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>().enabled);
        }

        [UnityTest]
        public IEnumerator GLTFShapeWithCollisionsUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb" // this glb has the "..._collider" object inside with the pre-defined collision geometry.
            }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            var colliderObject = scene.entities[entityId].gameObject.GetComponentInChildren<Collider>();
            Assert.IsTrue(colliderObject != null);
            Assert.IsTrue(colliderObject.GetComponent<MeshRenderer>() == null);
        }
    }
}
