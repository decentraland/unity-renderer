using DCL;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Tests
{
    public abstract class APKWithRefCountedAssetShouldWorkWhen_Base<APKType, AssetPromiseType, AssetType, AssetLibraryType> : TestsBase
        where AssetPromiseType : AssetPromise<AssetType>
        where AssetType : Asset, new()
        where AssetLibraryType : AssetLibrary_RefCounted<AssetType>, new()
        where APKType : AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType>, new()
    {
        protected APKType keeper;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            keeper = new APKType();
            yield break;
        }


        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            keeper.Cleanup();
            yield break;
        }


        protected abstract AssetPromiseType CreatePromise();

        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenLoadingAsset()
        {
            var prom = CreatePromise();
            bool calledSuccess = false;
            bool calledFail = false;

            prom.OnSuccessEvent +=
                (x) => { calledSuccess = true; };

            prom.OnFailEvent +=
                (x) => { calledFail = true; };

            keeper.Keep(prom);
            keeper.Forget(prom);
            keeper.Keep(prom);
            keeper.Forget(prom);

            Assert.IsTrue(prom != null);
            Assert.IsTrue(prom.asset == null);
            Assert.IsFalse(calledSuccess);
            Assert.IsTrue(!calledFail);

            keeper.library.Cleanup();
            yield break;
        }

        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenReusingAsset()
        {
            var prom = CreatePromise();
            AssetType loadedAsset = null;

            prom.OnSuccessEvent +=
                (x) => { loadedAsset = x; };

            keeper.Keep(prom);
            yield return prom;

            keeper.Forget(prom);
            keeper.Keep(prom);
            keeper.Forget(prom);

            Assert.IsTrue(prom.asset == null);
            keeper.library.Cleanup();
        }


        [UnityTest]
        public IEnumerator AnyAssetIsLoadedAndThenUnloaded()
        {
            var prom = CreatePromise();
            AssetType loadedAsset = null;


            prom.OnSuccessEvent +=
                (x) => { loadedAsset = x; };

            keeper.Keep(prom);

            Assert.AreEqual(AssetPromiseState.LOADING, prom.state);

            yield return prom;

            Assert.IsTrue(loadedAsset != null);
            Assert.IsTrue(keeper.library.Contains(loadedAsset));
            Assert.AreEqual(1, keeper.library.masterAssets.Count);

            keeper.Forget(prom);

            yield return prom;

            Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

            Assert.IsTrue(!keeper.library.Contains(loadedAsset.id));
            Assert.AreEqual(0, keeper.library.masterAssets.Count);
            keeper.library.Cleanup();
        }

        [UnityTest]
        public IEnumerator ForgetIsCalledWhileAssetIsBeingLoaded()
        {
            var prom = CreatePromise();
            AssetType asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            keeper.Keep(prom);

            yield return new WaitForSeconds(0.1f);

            keeper.Forget(prom);

            Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

            var prom2 = CreatePromise();

            keeper.Keep(prom2);

            yield return prom2;

            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);

            keeper.Forget(prom2);

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

            //NOTE(Brian): We expect them to be the same asset because AssetBundle non-gameObject assets are shared, as opposed to instanced.
            Assert.IsTrue(asset == asset2);
            Assert.IsTrue(asset == asset3);
            Assert.IsTrue(asset2 == asset3);

            Assert.IsTrue(keeper.library.Contains(asset));
            Assert.AreEqual(1, keeper.library.masterAssets.Count);

            keeper.library.Cleanup();
        }
    }
}