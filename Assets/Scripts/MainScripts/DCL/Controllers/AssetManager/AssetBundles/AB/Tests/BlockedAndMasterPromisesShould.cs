using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundle_Tests
{
    public class BlockedAndMasterPromisesShould :
        TestsBase_APK<AssetPromiseKeeper_AB, AssetPromise_AB, Asset_AB, AssetLibrary_AB>
    {
        protected AssetPromise_AB CreatePromise(string hash = null)
        {
            string contentUrl = Utils.GetTestsAssetsPath() + "/AssetBundles/";
            hash = hash ?? "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
            var prom = new AssetPromise_AB(contentUrl, hash);
            return prom;
        }

        [UnityTest]
        public IEnumerator FailCorrectlyWhenGivenWrongURL()
        {
            string url = "non_existing_url.glb";

            AssetPromise_AB prom = CreatePromise(url);
            Asset_AB asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            AssetPromise_AB prom2 = CreatePromise(url);
            Asset_AB asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            AssetPromise_AB prom3 = CreatePromise(url);
            Asset_AB asset3 = null;
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
