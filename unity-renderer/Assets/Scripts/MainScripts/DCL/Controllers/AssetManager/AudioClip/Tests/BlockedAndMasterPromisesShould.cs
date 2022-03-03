using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AudioClip_Tests
{
    public class BlockedAndMasterPromisesShould : TestsBase_APK<AssetPromiseKeeper_AudioClip,
        AssetPromise_AudioClip,
        Asset_AudioClip,
        AssetLibrary_RefCounted<Asset_AudioClip>>
    {
        [UnityTest]
        public IEnumerator AudioClip_FailCorrectlyWhenGivenWrongURL()
        {
            var provider = new ContentProvider_Dummy();

            var prom = new AssetPromise_AudioClip("123325", provider);
            Asset_AudioClip asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x, error) => { failEventCalled1 = true; };

            var prom2 = new AssetPromise_AudioClip("43254378", provider);
            Asset_AudioClip asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x, error) => { failEventCalled2 = true; };

            var prom3 = new AssetPromise_AudioClip("09898765", provider);
            Asset_AudioClip asset3 = null;
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