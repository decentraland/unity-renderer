using DCL;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_GLTF_Tests
{
    public class AnyAssetPromiseShould : IntegrationTestSuite_Legacy
    {
        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            var keeper = new AssetPromiseKeeper_GLTF();

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF loadedAsset = null;


            prom.OnSuccessEvent +=
                (x) => { loadedAsset = x; }
                ;

            Vector3 initialPos = Vector3.one;
            Quaternion initialRot = Quaternion.LookRotation(Vector3.right, Vector3.up);
            Vector3 initialScale = Vector3.one * 2;

            prom.settings.initialLocalPosition = initialPos;
            prom.settings.initialLocalRotation = initialRot;
            prom.settings.initialLocalScale = initialScale;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsTrue(PoolManager.i.ContainsPool(loadedAsset.id), "Not in pool after loaded!");

            Pool pool = PoolManager.i.GetPool(loadedAsset.id);

            Assert.AreEqual(0, pool.unusedObjectsCount, "incorrect inactive objects in pool");
            Assert.AreEqual(1, pool.usedObjectsCount, "incorrect active objects in pool");
            Assert.IsTrue(pool.original != loadedAsset.container, "In pool, the original gameObject must NOT be the loaded asset!");

            //NOTE(Brian): If the following asserts fail, check that ApplySettings_LoadStart() is called from AssetPromise_GLTF.AddToLibrary() when the clone is made.
            Assert.AreEqual(initialPos.ToString(), loadedAsset.container.transform.localPosition.ToString(), "initial position not set correctly!");
            Assert.AreEqual(initialRot.ToString(), loadedAsset.container.transform.localRotation.ToString(), "initial rotation not set correctly!");
            Assert.AreEqual(initialScale.ToString(), loadedAsset.container.transform.localScale.ToString(), "initial scale not set correctly!");

            Assert.IsTrue(loadedAsset != null);
            Assert.IsTrue(keeper.library.Contains(loadedAsset));
            Assert.AreEqual(1, keeper.library.masterAssets.Count);
            keeper.Cleanup();
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOff()
        {
            var keeper = new AssetPromiseKeeper_GLTF();

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            prom.settings.forceNewInstance = false;
            keeper.Keep(prom);
            yield return prom;

            var poolableObjectComponent = PoolManager.i.GetPoolable(prom.asset.container);
            Assert.IsNotNull(poolableObjectComponent);
            keeper.Cleanup();
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOffMultipleTimes()
        {
            var keeper = new AssetPromiseKeeper_GLTF();

            var poolableComponents = new List<PoolableObject>();

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            for (int i = 0; i < 10; i++)
            {
                AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
                prom.settings.forceNewInstance = false;
                keeper.Keep(prom);
                yield return prom;
                poolableComponents.Add(PoolManager.i.GetPoolable(prom.asset.container));
                keeper.Forget(prom);
            }

            Assert.IsTrue(poolableComponents.TrueForAll(x => x != null));
            keeper.Cleanup();
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOn()
        {
            var keeper = new AssetPromiseKeeper_GLTF();

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            prom.settings.forceNewInstance = true;

            keeper.Keep(prom);
            yield return prom;

            var poolableObjectComponent = PoolManager.i.GetPoolable(prom.asset.container);
            Assert.IsNull(poolableObjectComponent);

            if (prom.asset.container != null)
                Object.Destroy(prom.asset.container);

            keeper.Cleanup();
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOnMultipleTimes()
        {
            var keeper = new AssetPromiseKeeper_GLTF();

            var poolableComponents = new List<PoolableObject>();

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";

            for (int i = 0; i < 10; i++)
            {
                AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
                prom.settings.forceNewInstance = true;
                keeper.Keep(prom);
                yield return prom;
                poolableComponents.Add(PoolManager.i.GetPoolable(prom.asset.container));
                keeper.Forget(prom);
            }

            Assert.IsTrue(poolableComponents.TrueForAll(x => x == null));

            keeper.Cleanup();
        }
    }
}