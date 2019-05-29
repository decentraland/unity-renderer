using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

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

            Assert.IsTrue(scene.entities[entityId].meshGameObject == null,
                "Since the shape hasn't been updated yet, the child mesh shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.OBJ_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/OBJ/teapot.obj"
                }));

            OBJLoader objShape = scene.entities[entityId].gameObject.GetComponentInChildren<OBJLoader>(true);
            yield return new WaitUntil(() => objShape.alreadyLoaded);

            Assert.IsTrue(scene.entities[entityId].meshGameObject != null,
                "Every entity with a shape should have the mandatory 'Mesh' object as a child");

            var childRenderer = scene.entities[entityId].meshGameObject.GetComponentInChildren<MeshRenderer>();
            Assert.IsTrue(childRenderer != null,
                "Since the shape has already been updated, the child renderer should exist");
            Assert.AreNotSame(placeholderLoadingMaterial, childRenderer.sharedMaterial,
                "Since the shape has already been updated, the child renderer found shouldn't have the 'AssetLoading' placeholder material");
        }

        [UnityTest]
        public IEnumerator GLTFShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
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

            var meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);

            // Update its shape to a cylinder
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");

            Assert.IsTrue(entity.meshGameObject != null, "meshGameObject should not be null");

            meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsTrue(entity.meshGameObject.GetComponent<MeshFilter>() != null,
                "After updating the entity shape to a basic shape, the mesh filter shouldn't be removed from the object");

            Assert.IsTrue(entity.currentShape != null, "current shape must exist 1");
            Assert.IsTrue(entity.currentShape is CylinderShape, "current shape is BoxShape");

            // Update its shape to a GLTF
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(entity.currentShape != null, "current shape must exist 2");
            Assert.IsTrue(entity.currentShape is GLTFShape, "current shape is GLTFShape");

            Assert.IsTrue(entity.meshGameObject != null);

            Assert.IsTrue(entity.meshGameObject.GetComponent<MeshFilter>() == null,
                "After updating the entity shape to a GLTF shape, the mesh filter should be removed from the object");
            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            // Update its shape to a sphere
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");

            yield return null;

            Assert.IsTrue(entity.meshGameObject != null);

            meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component shouldn't exist after the shape is updated to a non-GLTF shape");
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
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

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
                }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            {
                var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

                Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in second object!");
                Assert.IsTrue(gltfObject.transform.Find("node_damagedHelmet_-6514") != null,
                    "Can't find \"node_damagedHelmet_-6514\"!");
            }
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeImmediateUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
                }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.AreEqual(1,
                scene.entities[entityId].gameObject.GetComponentsInChildren<InstantiatedGLTFObject>().Length,
                "Only 1 'InstantiatedGLTFObject' should remain once the GLTF shape has been updated");
        }

        [UnityTest]
        public IEnumerator GLTFShapeWithCollisionsUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() +
                          "/GLB/PalmTree_01.glb" // this glb has the "..._collider" object inside with the pre-defined collision geometry.
                }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            var colliderObject = scene.entities[entityId].gameObject.GetComponentInChildren<Collider>();
            Assert.IsTrue(colliderObject != null);
        }

        [UnityTest]
        public IEnumerator GLTFShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return InitScene();

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            // set first GLTF
            string gltfId1 = TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            var gltf1 = scene.GetSharedComponent(gltfId1);

            GLTFLoader gltfShape = entity.gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.AreEqual(gltf1, entity.GetSharedComponent(typeof(BaseShape)));

            // set second GLTF
            string gltfId2 = TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            gltfShape = entity.gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.AreEqual(scene.GetSharedComponent(gltfId2), entity.GetSharedComponent(typeof(BaseShape)));
            Assert.IsFalse(gltf1.attachedEntities.Contains(entity));
        }

        [UnityTest]
        public IEnumerator GLTFVisibilityDefault()
        {
            #region Arrange

            yield return InitScene();

            BaseLoadableShape<GLTFLoader>.Model gltfModel = new BaseLoadableShape<GLTFLoader>.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb",
            };

            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, gltfModel);
            yield return gltfShape.routine;

            GLTFLoader gltfLoader = scene.entities[gltfShape.attachedEntities.First().entityId].gameObject
                .GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            #endregion

            #region Act

            //EMPTY

            #endregion

            #region Assert

            Assert.AreEqual(true, gltfShape.model.visible);
            Assert.IsTrue(CheckVisibility(gltfShape, true));

            #endregion
        }

        [UnityTest]
        public IEnumerator GLTFVisibilityCreateTrue()
        {
            #region Arrange

            yield return InitScene();

            BaseLoadableShape<GLTFLoader>.Model gltfModel = new BaseLoadableShape<GLTFLoader>.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb",
                visible = true
            };

            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, gltfModel);
            yield return gltfShape.routine;

            GLTFLoader gltfLoader = scene.entities[gltfShape.attachedEntities.First().entityId].gameObject
                .GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            #endregion

            #region Act

            //EMPTY

            #endregion

            #region Assert

            Assert.AreEqual(true, gltfShape.model.visible);
            Assert.IsTrue(CheckVisibility(gltfShape, true));

            #endregion
        }

        [UnityTest]
        public IEnumerator GLTFVisibilityCreateFalse()
        {
            #region Arrange

            yield return InitScene();

            BaseLoadableShape<GLTFLoader>.Model gltfModel = new BaseLoadableShape<GLTFLoader>.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb",
                visible = false
            };

            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, gltfModel);
            yield return gltfShape.routine;

            GLTFLoader gltfLoader = scene.entities[gltfShape.attachedEntities.First().entityId].gameObject
                .GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            #endregion

            #region Act

            //EMPTY

            #endregion

            #region Assert

            Assert.AreEqual(false, gltfShape.model.visible);
            Assert.IsTrue(CheckVisibility(gltfShape, false));

            #endregion
        }

        [UnityTest]
        public IEnumerator GLTFVisibilityUpdateFalse()
        {
            #region Arrange

            yield return InitScene();

            BaseLoadableShape<GLTFLoader>.Model gltfModel = new BaseLoadableShape<GLTFLoader>.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb",
                visible = true
            };

            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, gltfModel);
            yield return gltfShape.routine;

            GLTFLoader gltfLoader = scene.entities[gltfShape.attachedEntities.First().entityId].gameObject
                .GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            #endregion

            #region Act

            gltfModel.visible = false;
            TestHelpers.SharedComponentUpdate(scene, gltfShape, gltfModel);

            #endregion

            #region Assert

            Assert.AreEqual(false, gltfShape.model.visible);
            Assert.IsTrue(CheckVisibility(gltfShape, false));

            #endregion
        }

        [UnityTest]
        public IEnumerator GLTFVisibilityUpdateTrue()
        {
            #region Arrange

            yield return InitScene();

            BaseLoadableShape<GLTFLoader>.Model gltfModel = new BaseLoadableShape<GLTFLoader>.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb",
                visible = false
            };

            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, gltfModel);
            yield return gltfShape.routine;

            GLTFLoader gltfLoader = scene.entities[gltfShape.attachedEntities.First().entityId].gameObject
                .GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            #endregion

            #region Act

            gltfModel.visible = true;
            TestHelpers.SharedComponentUpdate(scene, gltfShape, gltfModel);

            #endregion

            #region Assert

            Assert.AreEqual(true, gltfShape.model.visible);
            Assert.IsTrue(CheckVisibility(gltfShape, true));

            #endregion
        }

        [UnityTest]
        public IEnumerator GLTFVisibilityUpdateMixed()
        {
            #region Arrange

            yield return InitScene();

            BaseLoadableShape<GLTFLoader>.Model lanternModel = new BaseLoadableShape<GLTFLoader>.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb",
                visible = false
            };
            GLTFShape lanternShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, lanternModel);
            yield return lanternShape.routine;

            GLTFLoader gltfLoader = scene.entities[lanternShape.attachedEntities.First().entityId].gameObject
                .GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            BaseLoadableShape<GLTFLoader>.Model palmModel = new BaseLoadableShape<GLTFLoader>.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb",
                visible = true
            };
            GLTFShape palmShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, palmModel);
            yield return palmShape.routine;

            gltfLoader = scene.entities[palmShape.attachedEntities.First().entityId].gameObject
                .GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            #endregion

            #region Act

            lanternModel.visible = true;
            TestHelpers.SharedComponentUpdate(scene, lanternShape, lanternModel);

            palmModel.visible = false;
            TestHelpers.SharedComponentUpdate(scene, palmShape, palmModel);

            #endregion

            #region Assert

            Assert.AreEqual(true, lanternShape.model.visible);
            Assert.IsTrue(CheckVisibility(lanternShape, true));

            Assert.AreEqual(false, palmShape.model.visible);
            Assert.IsTrue(CheckVisibility(palmShape, false));

            #endregion
        }

        private bool CheckVisibility(BaseDisposable shapeComponent, bool isVisible)
        {
            var meshGameObjects = shapeComponent.attachedEntities.Select(x => x.meshGameObject);

            foreach (GameObject meshGameObject in meshGameObjects)
            {
                MeshFilter[] meshFilters = meshGameObject.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    MeshRenderer renderer = meshFilter.GetComponent<MeshRenderer>();
                    if (renderer != null && isVisible != renderer.enabled)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}