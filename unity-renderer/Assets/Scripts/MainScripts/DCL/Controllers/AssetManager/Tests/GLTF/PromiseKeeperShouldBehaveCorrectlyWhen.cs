using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_GLTF_Tests
{
    public class PromiseKeeperShouldBehaveCorrectlyWhen : TestsBase
    {
        [UnityTest]
        public IEnumerator AnyAssetIsDestroyedWhileLoading()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_GLTF();
            var keeper = new AssetPromiseKeeper_GLTF(library);

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);

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
        public IEnumerator ForgetIsCalledWhileAssetIsBeingReused()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_GLTF();
            var keeper = new AssetPromiseKeeper_GLTF(library);

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            bool calledFail = false;

            keeper.Keep(prom);
            yield return prom;

            prom.asset.container.name = "First GLTF";

            AssetPromise_GLTF prom2 = new AssetPromise_GLTF(scene.contentProvider, url);

            prom2.OnFailEvent +=
                (x) =>
                {
                    calledFail = true;
                };

            keeper.Keep(prom2);
            GameObject container = prom2.asset.container;
            keeper.Forget(prom2);

            yield return prom2;

            Assert.IsTrue(prom2 != null);
            Assert.IsTrue(calledFail);
            Assert.IsTrue(prom2.asset == null, "Asset shouldn't exist after Forget!");
            Assert.IsTrue(container != null, "Container should be pooled!");

            PoolableObject po = container.GetComponentInChildren<PoolableObject>(true);

            Assert.IsTrue(po.isInsidePool, "Asset should be inside pool!");
        }


        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenLoadingAsset()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_GLTF();
            var keeper = new AssetPromiseKeeper_GLTF(library);

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
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
            Assert.IsFalse(calledSuccess);
            Assert.IsTrue(calledFail);
        }

        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenReusingAsset()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_GLTF();
            var keeper = new AssetPromiseKeeper_GLTF(library);

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF loadedAsset = null;

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


        [UnityTest]
        public IEnumerator AnyAssetIsLoadedAndThenUnloaded()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_GLTF();
            var keeper = new AssetPromiseKeeper_GLTF(library);

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF loadedAsset = null;


            prom.OnSuccessEvent +=
                (x) =>
                {
                    loadedAsset = x;
                };

            keeper.Keep(prom);

            Assert.IsTrue(prom.state == AssetPromiseState.LOADING);

            yield return prom;

            Assert.IsTrue(loadedAsset != null);
            //Assert.IsTrue(loadedAsset.isLoaded);
            Assert.IsTrue(library.Contains(loadedAsset));
            Assert.AreEqual(1, library.masterAssets.Count);

            keeper.Forget(prom);

            yield return prom;

            Assert.IsTrue(prom.state == AssetPromiseState.IDLE_AND_EMPTY);

            yield return MemoryManager.i.CleanupPoolsIfNeeded(forceCleanup: true);

            Assert.IsTrue(!library.Contains(loadedAsset.id));
            Assert.AreEqual(0, library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator ForgetIsCalledWhileAssetIsBeingLoaded()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_GLTF();
            var keeper = new AssetPromiseKeeper_GLTF(library);

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            keeper.Keep(prom);

            yield return new WaitForSeconds(0.5f);

            keeper.Forget(prom);

            Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

            AssetPromise_GLTF prom2 = new AssetPromise_GLTF(scene.contentProvider, url);

            keeper.Keep(prom2);

            yield return prom2;

            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);

            keeper.Forget(prom2);

            yield return MemoryManager.i.CleanupPoolsIfNeeded(forceCleanup: true);

            Assert.IsTrue(asset == null);
            Assert.IsTrue(!library.Contains(asset));
            Assert.AreEqual(0, library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator ManyPromisesWithTheSameURLAreLoaded()
        {
            yield return InitScene();

            var library = new AssetLibrary_GLTF();
            var keeper = new AssetPromiseKeeper_GLTF(library);

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";

            string id = "1";
            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            AssetPromise_GLTF prom2 = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF asset2 = null;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };

            AssetPromise_GLTF prom3 = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF asset3 = null;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            yield return prom;
            yield return new WaitForSeconds(2.0f);

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

            Assert.IsTrue(library.Contains(asset));
            Assert.AreEqual(1, library.masterAssets.Count);
        }
    }
}