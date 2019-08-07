using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

namespace Tests
{
    public class AssetManagerTests : TestsBase
    {
        [UnityTest]
        public IEnumerator GetSingleCachedGLTF()
        {
            yield return InitScene();

            DecentralandEntity e = TestHelpers.CreateSceneEntity(scene);

            Assert.IsTrue(e.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";

            LoadableShape.Model gltfModel =
                new LoadableShape.Model()
                {
                    src = url
                };

            TestHelpers.AttachGLTFShape(e, scene, Vector3.zero, gltfModel);

            LoadWrapper_GLTF gltfShape = e.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            GLTFShape newShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, gltfModel);

            yield return null;

            Assert.IsTrue(newShape != null, "newShape is null??");
            Assert.AreEqual(1, AssetManager_GLTF.i.assetLibrary.Count, "GLTF is not cached correctly!");
        }

        [UnityTest]
        public IEnumerator GetMultipleCachedGLTFAtOnce()
        {
            yield return InitScene();

            DecentralandEntity e = TestHelpers.CreateSceneEntity(scene);
            int currentDownloadingCount = GLTFComponent.downloadingCount;

            Assert.IsTrue(e.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";

            LoadableShape.Model gltfModel =
                new LoadableShape.Model()
                {
                    src = url
                };

            TestHelpers.AttachGLTFShape(e, scene, Vector3.zero, gltfModel);

            GLTFShape shape1 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 1, gltfModel);
            GLTFShape shape2 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 2, gltfModel);
            GLTFShape shape3 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 3, gltfModel);
            GLTFShape shape4 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 4, gltfModel);

            yield return null;

            Assert.IsTrue(GLTFComponent.downloadingCount == (currentDownloadingCount + 1),
                "Multiple GLTF loading detected, caching not working?");

            LoadWrapper_GLTF gltfShape = e.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(shape1 != null, "shape1 is null??");
            Assert.IsTrue(shape2 != null, "shape2 is null??");
            Assert.IsTrue(shape3 != null, "shape3 is null??");
            Assert.IsTrue(shape4 != null, "shape4 is null??");
            Assert.AreEqual(1, AssetManager_GLTF.i.assetLibrary.Count, "GLTF is not cached correctly!");
        }

        [UnityTest]
        public IEnumerator GLTFEntityDownloadingIsRemoved()
        {
            yield return InitScene();

            LoadableShape.Model palmModel = new LoadableShape.Model()
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb",
                visible = true
            };

            GLTFShape palmShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, palmModel, out DecentralandEntity entity1);
            GLTFShape palmShape2 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, palmModel, out DecentralandEntity entity2);
            GLTFShape palmShape3 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, palmModel, out DecentralandEntity entity3);

            object libraryEntryId = entity1.gameObject.GetComponentInChildren<LoadWrapper_GLTF>().GetCacheId();

            Assert.AreEqual(0, AssetManager_GLTF.i.transform.childCount);
            Assert.AreEqual(3, AssetManager_GLTF.i.assetLibrary[libraryEntryId].referenceCount);

            scene.RemoveEntity(entity1.entityId);

            Assert.AreEqual(1, AssetManager_GLTF.i.transform.childCount);
            Assert.AreEqual(2, AssetManager_GLTF.i.assetLibrary[libraryEntryId].referenceCount);

            yield return palmShape2.routine;
            yield return palmShape3.routine;

            Assert.NotNull(entity2.gameObject.GetComponentInChildren<LoadWrapper_GLTF>());
            Assert.NotNull(entity3.gameObject.GetComponentInChildren<LoadWrapper_GLTF>());
        }
    }
}
