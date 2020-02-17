using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundle_GameObject_Tests
{
    public class APK_AB_GameObject_Promise_Should : TestsBase_APK<AssetPromiseKeeper_AB_GameObject,
                                                AssetPromise_AB_GameObject,
                                                Asset_AB_GameObject,
                                                AssetLibrary_AB_GameObject>
    {
        protected AssetPromise_AB_GameObject CreatePromise(string hash = null)
        {
            string contentUrl = Utils.GetTestsAssetsPath() + "/AssetBundles/";
            hash = hash ?? "QmNS4K7GaH63T9rhAfkrra7ADLXSEeco8FTGknkPnAVmKM";
            var prom = new AssetPromise_AB_GameObject(contentUrl, hash);
            return prom;
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            var prom = CreatePromise();
            Asset_AB_GameObject loadedAsset = null;

            prom.OnSuccessEvent +=
                (x) =>
                {
                    Debug.Log("Success is called");
                    loadedAsset = x;
                }
            ;

            prom.OnFailEvent += (x) => { Debug.Log("Fail is called"); };

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
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOff()
        {
            var prom = CreatePromise();
            prom.settings.forceNewInstance = false;
            keeper.Keep(prom);
            yield return prom;

            var poolableObjectComponent = PoolManager.i.GetPoolable(prom.asset.container);
            Assert.IsNotNull(poolableObjectComponent);
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOffMultipleTimes()
        {
            var poolableComponents = new List<PoolableObject>();

            for (int i = 0; i < 10; i++)
            {
                var prom = CreatePromise();
                prom.settings.forceNewInstance = false;
                keeper.Keep(prom);
                yield return prom;
                poolableComponents.Add(PoolManager.i.GetPoolable(prom.asset.container));
                keeper.Forget(prom);
            }

            Assert.IsTrue(poolableComponents.TrueForAll(x => x != null));
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOn()
        {
            var prom = CreatePromise();
            prom.settings.forceNewInstance = true;
            keeper.Keep(prom);
            yield return prom;

            var poolableObjectComponent = PoolManager.i.GetPoolable(prom.asset.container);
            Assert.IsNull(poolableObjectComponent);
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOnMultipleTimes()
        {
            var poolableComponents = new List<PoolableObject>();

            for (int i = 0; i < 10; i++)
            {
                var prom = CreatePromise();
                prom.settings.forceNewInstance = true;
                keeper.Keep(prom);
                yield return prom;
                poolableComponents.Add(PoolManager.i.GetPoolable(prom.asset.container));
                keeper.Forget(prom);
            }

            Assert.IsTrue(poolableComponents.TrueForAll(x => x == null));
        }
    }
}
