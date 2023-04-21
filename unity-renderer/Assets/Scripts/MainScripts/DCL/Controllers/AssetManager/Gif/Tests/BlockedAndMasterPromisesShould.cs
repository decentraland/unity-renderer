using System;
using AssetPromiseKeeper_Tests;
using DCL;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Gif_Tests
{
    public class BlockedAndMasterPromisesShould : TestsBase_APK<AssetPromiseKeeper_Gif,
        AssetPromise_Gif,
        Asset_Gif,
        AssetLibrary_RefCounted<Asset_Gif>>
    {
        [UnityTest]
        public IEnumerator Gif_FailCorrectlyWhenGivenWrongURL()
        {
            string url1 = "123325";
            string url2 = "43254378";
            string url3 = "09898765";
            
            var prom = new AssetPromise_Gif(url1);
            Asset_Gif asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x, error) => { failEventCalled1 = true; };

            var prom2 = new AssetPromise_Gif(url2);
            Asset_Gif asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x, error) => { failEventCalled2 = true; };

            var prom3 = new AssetPromise_Gif(url3);
            Asset_Gif asset3 = null;
            bool failEventCalled3 = false;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };
            prom3.OnFailEvent += (x, error) => { failEventCalled3 = true; };

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