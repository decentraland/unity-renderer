using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
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

            LoadWrapper_OBJ objShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_OBJ>(true);
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

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
        }

        [UnityTest]
        public IEnumerator NFTShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            var entity = scene.entities[entityId];
            Assert.IsTrue(entity.meshGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };

            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity);

            Assert.IsTrue(entity.meshGameObject != null, "entity mesh object should already exist as the NFTShape already initialized");

            var nftShape = entity.meshGameObject.GetComponent<LoadWrapper_NFT>();
            var backgroundMaterialPropertyBlock = new MaterialPropertyBlock();
            nftShape.loaderController.meshRenderer.GetPropertyBlock(backgroundMaterialPropertyBlock, 1);

            Assert.IsTrue(backgroundMaterialPropertyBlock.GetColor("_BaseColor") == new Color(0.6404918f, 0.611472f, 0.8584906f), "The NFT frame background color should be the default one");

            // Update color and check if it changed
            componentModel.color = Color.yellow;
            yield return TestHelpers.SharedComponentUpdate(component, componentModel);

            nftShape.loaderController.meshRenderer.GetPropertyBlock(backgroundMaterialPropertyBlock, 1);
            Assert.AreEqual(Color.yellow, backgroundMaterialPropertyBlock.GetColor("_BaseColor"), "The NFT frame background color should be yellow");
        }

        [UnityTest]
        public IEnumerator NFTShapeMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            var component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE);
            yield return component.routine;

            Assert.IsFalse(component == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<NFTShape.Model, NFTShape>(scene, CLASS_ID.NFT_SHAPE);
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

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
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

            var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            {
                var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

                Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in first object!");
                Assert.IsTrue(gltfObject.transform.Find("Lantern") != null, "Can't find \"Lantern!\"");
            }

            TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
                }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
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

            var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
                }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
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

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
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

            LoadWrapper_GLTF gltfLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            Assert.AreEqual(gltf1, entity.GetSharedComponent(typeof(BaseShape)));

            // set second GLTF
            string gltfId2 = TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            gltfLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            Assert.AreEqual(scene.GetSharedComponent(gltfId2), entity.GetSharedComponent(typeof(BaseShape)));
            Assert.IsFalse(gltf1.attachedEntities.Contains(entity));
        }



        [UnityTest]
        public IEnumerator GLTFVisibleProperty()
        {
            yield return InitScene();

            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];
            yield return null;

            // Create shape component
            var shapeModel = new LoadableShape<LoadWrapper_GLTF>.Model();
            shapeModel.src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb";

            var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF>, LoadableShape<LoadWrapper_GLTF>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);
        }

        [UnityTest]
        public IEnumerator NFTShapeVisibleProperty()
        {
            yield return InitScene();

            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];
            yield return null;

            // Create shape component
            var shapeModel = new LoadableShape<LoadWrapper_NFT>.Model();
            shapeModel.src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536";

            var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_NFT>, LoadableShape<LoadWrapper_NFT>.Model>(scene, CLASS_ID.NFT_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);
        }
    }
}
