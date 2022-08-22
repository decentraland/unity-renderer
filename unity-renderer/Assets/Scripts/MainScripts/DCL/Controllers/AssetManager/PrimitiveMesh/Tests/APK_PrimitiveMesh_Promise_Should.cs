using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_PrimitiveMesh_Tests
{
    public class APK_PrimitiveMesh_Promise_Should : TestsBase_APK<AssetPromiseKeeper_PrimitiveMesh,
        AssetPromise_PrimitiveMesh,
        Asset_PrimitiveMesh,
        AssetLibrary_RefCounted<Asset_PrimitiveMesh>>
    {
        private AssetPromise_PrimitiveMesh CreatePromise()
        {
            var prom = new AssetPromise_PrimitiveMesh(AssetPromise_PrimitiveMesh_Model.CreateBox(null));
            return prom;
        }

        private AssetPromise_PrimitiveMesh CreatePromise(AssetPromise_PrimitiveMesh_Model model)
        {
            var prom = new AssetPromise_PrimitiveMesh(model);
            return prom;
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            Asset_PrimitiveMesh loadedAsset = null;
            var prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);

            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);
        }

        [UnityTest]
        public IEnumerator ShareMeshAmongPromisesWithSameSettings()
        {
            Asset_PrimitiveMesh loadedAsset = null;
            var prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_PrimitiveMesh loadedAsset2 = null;
            var prom2 = CreatePromise();

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.mesh);

            Assert.IsTrue(loadedAsset.mesh == loadedAsset2.mesh);
        }

        [UnityTest]
        public IEnumerator NotShareMeshAmongPromisesWithDifferentSettings()
        {
            AssetPromise_PrimitiveMesh_Model model = AssetPromise_PrimitiveMesh_Model.CreateBox(null);
            AssetPromise_PrimitiveMesh_Model model2 = AssetPromise_PrimitiveMesh_Model.CreateBox(
                new float[]
                {
                    0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                    0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                    0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                    0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                    0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                    0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1
                });
            Asset_PrimitiveMesh loadedAsset = null;
            var prom = CreatePromise(model);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_PrimitiveMesh loadedAsset2 = null;
            var prom2 = CreatePromise(model2);

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.mesh);

            Assert.IsTrue(loadedAsset.mesh != loadedAsset2.mesh);
        }

        [UnityTest]
        public IEnumerator KeepRefCountCorrectly()
        {
            AssetPromise_PrimitiveMesh_Model model = AssetPromise_PrimitiveMesh_Model.CreateBox(null);
            var prom = new AssetPromise_PrimitiveMesh(model);
            keeper.Keep(prom);
            yield return prom;

            Assert.AreEqual(1, keeper.library.masterAssets[model].referenceCount);

            var prom2 = new AssetPromise_PrimitiveMesh(model);
            keeper.Keep(prom2);
            yield return prom2;

            Assert.AreEqual(2, keeper.library.masterAssets[model].referenceCount);

            keeper.Forget(prom);
            Assert.AreEqual(1, keeper.library.masterAssets[model].referenceCount);

            prom = new AssetPromise_PrimitiveMesh(model);
            keeper.Keep(prom);
            yield return prom;

            Assert.AreEqual(2, keeper.library.masterAssets[model].referenceCount);
            keeper.Forget(prom);
            keeper.Forget(prom2);

            Assert.AreEqual(0, keeper.library.masterAssets.Count);
        }
    }
}