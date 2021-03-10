using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Mock_Tests
{
    public class BlockedAndMasterPromisesShould
    {
        [UnityTest]
        public IEnumerator ResolveCorrectlyIfKeepIsCalledWhenBlockedPromisesAreBeingProcessed()
        {
            Time.maximumDeltaTime = 0.016f;

            var library = new AssetLibrary_Mock();
            var keeper = new AssetPromiseKeeper_Mock(library);

            string id = "1";

            var promList = new List<AssetPromise_Mock>();

            var mischievousPromise = new AssetPromise_Mock();
            mischievousPromise.idGenerator = id;
            mischievousPromise.loadTime = 0.01f;

            var mischievousPromise2 = new AssetPromise_Mock();
            mischievousPromise2.idGenerator = id;
            mischievousPromise2.loadTime = 0.01f;

            for (int i = 0; i < 49; i++)
            {
                AssetPromise_Mock tmpProm = new AssetPromise_Mock();
                tmpProm.idGenerator = id;
                tmpProm.loadTime = 0.01f;
                keeper.Keep(tmpProm);
                promList.Add(tmpProm);
            }

            for (int i = 0; i < promList.Count; i++)
            {
                AssetPromise_Mock prom = promList[i];
                yield return prom;

                if (i == 25)
                {
                    keeper.Keep(mischievousPromise);
                    keeper.Keep(mischievousPromise2);
                    yield return new DCL.WaitUntil(() => mischievousPromise.keepWaiting == false, 2.0f);
                    yield return new DCL.WaitUntil(() => mischievousPromise2.keepWaiting == false, 2.0f);
                    Assert.IsFalse(mischievousPromise.keepWaiting, "While blocked promises are being resolved, new promises enqueued with the same id should solve correctly!");
                    Assert.IsFalse(mischievousPromise2.keepWaiting, "While blocked promises are being resolved, new promises enqueued with the same id should solve correctly!");
                }
            }
        }

        [UnityTest]
        public IEnumerator FailCorrectlyIfMasterPromiseFails()
        {
            var library = new AssetLibrary_Mock();
            var keeper = new AssetPromiseKeeper_Mock(library);

            string id = "1";
            AssetPromise_Mock prom = new AssetPromise_Mock();
            prom.idGenerator = id;
            Asset_Mock asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            prom.forceFail = true;

            AssetPromise_Mock prom2 = new AssetPromise_Mock();
            prom2.idGenerator = id;
            Asset_Mock asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            AssetPromise_Mock prom3 = new AssetPromise_Mock();
            prom3.idGenerator = id;
            Asset_Mock asset3 = null;
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

            Assert.AreEqual(0, keeper.waitingPromisesCount);

            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsTrue(failEventCalled1);
            Assert.IsTrue(failEventCalled2);
            Assert.IsTrue(failEventCalled3);

            Assert.IsFalse(asset != null);
            Assert.IsFalse(asset2 != null);
            Assert.IsFalse(asset3 != null);

            Assert.IsFalse(library.Contains(asset));
            Assert.AreNotEqual(1, library.masterAssets.Count);

            AssetPromise_Mock prom4 = new AssetPromise_Mock();
            prom4.idGenerator = id;
            Asset_Mock asset4 = null;
            prom4.OnSuccessEvent += (x) => { asset4 = x; };

            keeper.Keep(prom4);

            yield return prom4;

            Assert.IsTrue(asset4 != null);
            Assert.IsTrue(library.Contains(asset4));
            Assert.AreEqual(1, library.masterAssets.Count);
        }

    }
}
