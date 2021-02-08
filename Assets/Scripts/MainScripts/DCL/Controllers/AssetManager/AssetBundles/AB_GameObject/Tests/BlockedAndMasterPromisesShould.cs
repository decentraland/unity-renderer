using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundle_GameObject_Tests
{
    public class BlockedAndMasterPromisesShould : TestsBase_APK<AssetPromiseKeeper_AB_GameObject,
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

        protected override IEnumerator TearDown()
        {
            yield return base.TearDown();

            // AssetPromise_AB_GameObject promises are wired to AssetPromiseKeeper_AB.
            // TODO(Brian): Pass the APK instance by ref to properly test this without this line.
            AssetPromiseKeeper_AB.i.Cleanup();
        }

        /// <summary>
        /// If this test fails, you should ensure that OnSilentForget is implemented properly.
        /// OnSilentForget should unparent and hide the container to ensure blocked promises finish correctly.
        /// </summary>
        [UnityTest]
        public IEnumerator SucceedWhenMastersParentIsDestroyed()
        {
            GameObject parent = new GameObject("parent");

            var prom = CreatePromise();
            prom.settings.parent = parent.transform;

            var prom2 = CreatePromise();
            bool failEventCalled2 = false;
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            var prom3 = CreatePromise();
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
            string invalidHash = "Qm_InVaLiD_hAsH";

            var prom = CreatePromise(invalidHash);
            Asset_AB_GameObject asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            var prom2 = CreatePromise(invalidHash);
            Asset_AB_GameObject asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            var prom3 = CreatePromise(invalidHash);
            Asset_AB_GameObject asset3 = null;
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
