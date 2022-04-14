using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using UnityGLTF;

namespace AssetPromiseKeeper_GLTF_Tests
{
    public class AnyAssetPromiseShould : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            var keeper = new AssetPromiseKeeper_GLTF();
            keeper.throttlingCounter.enabled = false;

            string url = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
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

            object poolId = keeper.library.AssetIdToPoolId(loadedAsset.id);
            Assert.IsTrue(PoolManager.i.ContainsPool(poolId), "Not in pool after loaded!");

            Pool pool = PoolManager.i.GetPool(poolId);

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
            keeper.throttlingCounter.enabled = false;

            string url = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
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
            keeper.throttlingCounter.enabled = false;

            var poolableComponents = new List<PoolableObject>();

            string url = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
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
            keeper.throttlingCounter.enabled = false;

            string url = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
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
            keeper.throttlingCounter.enabled = false;

            var poolableComponents = new List<PoolableObject>();

            string url = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";

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

        [UnityTest]
        public IEnumerator NotTryToLoadAfterForget()
        {
            var keeper = new AssetPromiseKeeper_GLTF();
            keeper.throttlingCounter.enabled = false;

            var promises = new List<AssetPromise_GLTF>();
            var forgottenPromises = new Dictionary<int, bool>();
            bool waitMasterPromise = true;

            string url = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb";

            AssetPromise_GLTF masterPromise = new AssetPromise_GLTF(scene.contentProvider, url);
            masterPromise.OnPreFinishEvent += promise => waitMasterPromise = false;
            keeper.Keep(masterPromise);

            for (int i = 0; i < 10; i++)
            {
                AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);

                promises.Add(prom);

                int promiseHash = prom.GetHashCode();
                forgottenPromises.Add(promiseHash, false);

                prom.OnSuccessEvent += (asset) =>
                {
                    Assert.IsFalse(forgottenPromises[promiseHash], "Success on forgotten promise shouldn't be called");
                };
                prom.OnFailEvent += (asset, error) =>
                {
                    Assert.IsFalse(forgottenPromises[promiseHash], "Fail on forgotten promise shouldn't be called");
                };
                keeper.Keep(prom);
            }

            keeper.Forget(masterPromise);

            yield return new WaitWhile(() => waitMasterPromise);
            yield return null;

            for (int i = 0; i < promises.Count; i++)
            {
                var prom = promises[i];
                forgottenPromises[prom.GetHashCode()] = true;
                keeper.Forget(prom);
            }

            keeper.Cleanup();
        }
    }
}