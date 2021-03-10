using DCL;
using DCL.Helpers;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_GLTF_Tests
{
    public class BlockedAndMasterPromisesShould : IntegrationTestSuite_Legacy
    {
        [UnityTest]
        public IEnumerator SucceedWhenMastersParentIsDestroyed()
        {
            var keeper = new AssetPromiseKeeper_GLTF();

            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            GameObject parent = new GameObject("parent");

            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            prom.settings.parent = parent.transform;

            AssetPromise_GLTF prom2 = new AssetPromise_GLTF(scene.contentProvider, url);
            bool failEventCalled2 = false;
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            AssetPromise_GLTF prom3 = new AssetPromise_GLTF(scene.contentProvider, url);
            bool failEventCalled3 = false;
            prom3.OnFailEvent += (x) => { failEventCalled3 = true; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            keeper.Forget(prom);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            Object.Destroy(parent);

            yield return prom;
            yield return prom2;
            yield return prom3;

            Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsFalse(failEventCalled2);
            Assert.IsFalse(failEventCalled3);

            Assert.IsTrue(prom.asset != null);
            Assert.IsTrue(prom2.asset != null);
            Assert.IsTrue(prom3.asset != null);

            Assert.IsTrue(keeper.library.Contains(prom.asset));
            Assert.AreEqual(1, keeper.library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator FailCorrectlyWhenGivenWrongURL()
        {
            var keeper = new AssetPromiseKeeper_GLTF();

            //NOTE(Brian): Expect the 404 error
            LogAssert.Expect(LogType.Error, new Regex("^*.?404"));

            string url = Utils.GetTestsAssetsPath() + "/non_existing_url.glb";

            AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            AssetPromise_GLTF prom2 = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            AssetPromise_GLTF prom3 = new AssetPromise_GLTF(scene.contentProvider, url);
            Asset_GLTF asset3 = null;
            bool failEventCalled3 = false;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };
            prom3.OnFailEvent += (x) => { failEventCalled3 = true; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            yield return prom;
            yield return prom2;
            yield return prom3;

            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsTrue(failEventCalled1);
            Assert.IsTrue(failEventCalled2);
            Assert.IsTrue(failEventCalled3);

            Assert.IsFalse(asset != null);
            Assert.IsFalse(asset2 != null);
            Assert.IsFalse(asset3 != null);

            Assert.IsFalse(keeper.library.Contains(asset));
            Assert.AreNotEqual(1, keeper.library.masterAssets.Count);
        }
    }
}
