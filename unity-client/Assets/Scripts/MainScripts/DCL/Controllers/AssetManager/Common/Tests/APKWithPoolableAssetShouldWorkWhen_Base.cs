using DCL;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Tests
{

    public abstract class APKWithPoolableAssetShouldWorkWhen_Base<APKType, AssetPromiseType, AssetType, AssetLibraryType>
                        : TestsBase_APK<APKType, AssetPromiseType, AssetType, AssetLibraryType>

        where AssetPromiseType : AssetPromise<AssetType>
        where AssetType : Asset_WithPoolableContainer, new()
        where AssetLibraryType : AssetLibrary_Poolable<AssetType>, new()
        where APKType : AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType>, new()
    {
        protected abstract AssetPromiseType CreatePromise();

        [UnityTest]
        public IEnumerator ForgetIsCalledWhileAssetIsBeingReused()
        {
            AssetPromiseType prom = CreatePromise();
            bool calledFail = false;

            keeper.Keep(prom);
            yield return prom;

            prom.asset.container.name = "First GLTF";

            AssetPromiseType prom2 = CreatePromise();

            prom2.OnFailEvent +=
                (x) =>
                {
                    calledFail = true;
                };

            keeper.Keep(prom2);
            GameObject container = prom2.asset.container;
            bool wasLoadingWhenForgotten = prom2.state == AssetPromiseState.LOADING;
            keeper.Forget(prom2);

            yield return prom2;

            Assert.IsTrue(prom2 != null);

            if (wasLoadingWhenForgotten)
                Assert.IsFalse(calledFail, "Fail event should NOT be called when forgotten while loading");

            Assert.IsTrue(prom2.asset == null, "Asset shouldn't exist after Forget!");
            Assert.IsTrue(container != null, "Container should be pooled!");

            PoolableObject po = PoolManager.i.GetPoolable(container);

            Assert.IsTrue(po.isInsidePool, "Asset should be inside pool!");
        }


        [UnityTest]
        public IEnumerator AnyAssetIsLoadedAndThenUnloaded()
        {
            var prom = CreatePromise();
            AssetType loadedAsset = null;


            prom.OnSuccessEvent +=
                (x) =>
                {
                    Debug.Log("success!");
                    loadedAsset = x;
                };

            keeper.Keep(prom);

            Assert.IsTrue(prom.state == AssetPromiseState.LOADING);

            yield return prom;

            Assert.IsTrue(loadedAsset != null);
            //Assert.IsTrue(loadedAsset.isLoaded);
            Assert.IsTrue(keeper.library.Contains(loadedAsset));
            Assert.AreEqual(1, keeper.library.masterAssets.Count);

            keeper.Forget(prom);

            yield return prom;

            Assert.IsTrue(prom.state == AssetPromiseState.IDLE_AND_EMPTY);

            Assert.IsTrue(keeper.library.Contains(loadedAsset.id), "Asset should be still in library, it only should be removed from library when the Pool is cleaned by the MemoryManager");
            Assert.AreEqual(1, keeper.library.masterAssets.Count, "Asset should be still in library, it only should be removed from library when the Pool is cleaned by the MemoryManager");

            yield return Environment.i.memoryManager.CleanupPoolsIfNeeded(true);

            Assert.AreEqual(0, keeper.library.masterAssets.Count, "After MemoryManager clear the pools, the asset should be removed from the library");
            Assert.IsTrue(!keeper.library.Contains(loadedAsset.id), "After MemoryManager clear the pools, the asset should be removed from the library");

        }

        [UnityTest]
        public IEnumerator AnyAssetIsDestroyedWhileLoading()
        {
            AssetPromiseType prom = CreatePromise();

            bool calledFail = false;

            prom.OnFailEvent +=
                (x) =>
                {
                    calledFail = true;
                };

            keeper.Keep(prom);
            yield return null;

            Object.Destroy(prom.asset.container);
            yield return prom;

            Assert.IsTrue(prom != null);
            Assert.IsTrue(prom.asset == null);
            Assert.IsTrue(calledFail);
        }

        [UnityTest]
        public IEnumerator ForgetIsCalledWhileAssetIsBeingLoaded()
        {
            var prom = CreatePromise();

            keeper.Keep(prom);

            yield return new WaitForSeconds(0.1f);

            keeper.Forget(prom);

            Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

            var prom2 = CreatePromise();

            AssetType asset = null;
            prom2.OnSuccessEvent += (x) => { asset = x; };

            keeper.Keep(prom2);

            yield return prom2;

            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);

            keeper.Forget(prom2);

            yield return Environment.i.memoryManager.CleanupPoolsIfNeeded(true);

            Assert.IsTrue(asset.container == null);
            Assert.IsTrue(!keeper.library.Contains(asset));
            Assert.AreEqual(0, keeper.library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator ManyPromisesWithTheSameURLAreLoaded()
        {
            var prom = CreatePromise();
            AssetType asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            var prom2 = CreatePromise();
            AssetType asset2 = null;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };

            var prom3 = CreatePromise();
            AssetType asset3 = null;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            yield return prom;
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(asset != null);
            Assert.IsTrue(asset2 != null);
            Assert.IsTrue(asset3 != null);

            Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsTrue(asset2.id == asset.id);
            Assert.IsTrue(asset3.id == asset.id);
            Assert.IsTrue(asset2.id == asset3.id);

            Assert.IsTrue(asset != asset2);
            Assert.IsTrue(asset != asset3);
            Assert.IsTrue(asset2 != asset3);

            Assert.IsTrue(keeper.library.Contains(asset));
            Assert.AreEqual(1, keeper.library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenLoadingAsset()
        {
            var prom = CreatePromise();
            bool calledSuccess = false;
            bool calledFail = false;

            prom.OnSuccessEvent +=
                (x) =>
                {
                    calledSuccess = true;
                };

            prom.OnFailEvent +=
                (x) =>
                {
                    calledFail = true;
                };

            keeper.Keep(prom);
            keeper.Forget(prom);
            keeper.Keep(prom);
            keeper.Forget(prom);

            Assert.IsTrue(prom != null);
            Assert.IsTrue(prom.asset == null);
            Assert.IsFalse(calledSuccess, "Success event should NOT be called when forgetting a promise");
            Assert.IsFalse(calledFail, "Fail event should NOT be called when forgetting a promise.");
            yield break;
        }

        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenReusingAsset()
        {
            var prom = CreatePromise();
            AssetType loadedAsset = null;

            prom.OnSuccessEvent +=
                (x) =>
                {
                    loadedAsset = x;
                };

            keeper.Keep(prom);
            yield return prom;

            keeper.Forget(prom);
            keeper.Keep(prom);
            keeper.Forget(prom);

            Assert.IsTrue(prom.asset == null);
        }
    }
}
