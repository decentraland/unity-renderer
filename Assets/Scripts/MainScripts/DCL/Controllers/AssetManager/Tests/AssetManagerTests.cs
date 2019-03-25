using DCL;
using DCL.Components;
using DCL.Controllers;
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

            Assert.IsTrue(e.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";

            BaseLoadableShape<GLTFLoader>.Model gltfModel =
                new BaseLoadableShape<GLTFLoader>.Model()
                {
                    src = url
                };

            TestHelpers.AttachGLTFShape(e, scene, Vector3.zero, gltfModel);

            yield return new WaitForSeconds(8f);

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

            Assert.IsTrue(e.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";

            BaseLoadableShape<GLTFLoader>.Model gltfModel =
                new BaseLoadableShape<GLTFLoader>.Model()
                {
                    src = url
                };

            TestHelpers.AttachGLTFShape(e, scene, Vector3.zero, gltfModel);

            GLTFShape shape1 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 1, gltfModel);
            GLTFShape shape2 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 2, gltfModel);
            GLTFShape shape3 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 3, gltfModel);
            GLTFShape shape4 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.right * 4, gltfModel);

            yield return null;

            Assert.IsTrue(GLTFComponent.downloadingCount == 1, "Multiple GLTF loading detected, caching not working?");

            yield return new WaitForSeconds(8f);

            Assert.IsTrue(shape1 != null, "shape1 is null??");
            Assert.IsTrue(shape2 != null, "shape2 is null??");
            Assert.IsTrue(shape3 != null, "shape3 is null??");
            Assert.IsTrue(shape4 != null, "shape4 is null??");
            Assert.AreEqual(1, AssetManager_GLTF.i.assetLibrary.Count, "GLTF is not cached correctly!");
        }
    }
}
