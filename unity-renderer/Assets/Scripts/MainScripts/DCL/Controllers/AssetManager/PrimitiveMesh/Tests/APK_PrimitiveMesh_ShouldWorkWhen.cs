using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_PrimitiveMesh_Tests
{
    public class APK_PrimitiveMesh_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_PrimitiveMesh,
        AssetPromise_PrimitiveMesh,
        Asset_PrimitiveMesh,
        AssetLibrary_RefCounted<Asset_PrimitiveMesh>>
    {
        protected override AssetPromise_PrimitiveMesh CreatePromise()
        {
            var prom = new AssetPromise_PrimitiveMesh(AssetPromise_PrimitiveMesh_Model.CreateBox(null));
            return prom;
        }

        [UnityTest]
        public override IEnumerator ManyPromisesWithTheSameURLAreLoaded()
        {
            var prom = CreatePromise();
            Asset_PrimitiveMesh asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            var prom2 = CreatePromise();
            Asset_PrimitiveMesh asset2 = null;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };

            var prom3 = CreatePromise();
            Asset_PrimitiveMesh asset3 = null;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

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

        [UnityTest]
        public override IEnumerator AnyAssetIsLoadedAndThenUnloaded()
        {
            var prom = CreatePromise();
            Asset_PrimitiveMesh loadedAsset = null;


            prom.OnSuccessEvent +=
                (x) => { loadedAsset = x; };

            keeper.Keep(prom);

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
        public override IEnumerator KeepAndForgetIsCalledInSingleFrameWhenLoadingAsset()
        {
            var prom = CreatePromise();
            bool calledSuccess = false;
            bool calledFail = false;

            prom.OnSuccessEvent +=
                (x) => { calledSuccess = true; };

            prom.OnFailEvent +=
                (x, error) => { calledFail = true; };

            keeper.Keep(prom);
            keeper.Forget(prom);
            keeper.Keep(prom);
            keeper.Forget(prom);

            Assert.IsTrue(prom != null);
            Assert.IsTrue(prom.asset == null);
            Assert.IsTrue(calledSuccess);

            keeper.library.Cleanup();
            yield break;
        }
    }
}