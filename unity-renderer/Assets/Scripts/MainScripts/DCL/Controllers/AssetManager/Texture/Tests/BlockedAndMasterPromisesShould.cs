using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Texture_Tests
{
    public class BlockedAndMasterPromisesShould : TestsBase_APK<AssetPromiseKeeper_Texture,
                                                            AssetPromise_Texture,
                                                            Asset_Texture,
                                                            AssetLibrary_Texture>
    {
        protected AssetPromise_Texture CreatePromise(string promiseURL, int wrapmode = -1, int filterMode = -1)
        {
            AssetPromise_Texture prom;

            if (filterMode > -1 && wrapmode > -1)
                prom = new AssetPromise_Texture(promiseURL, (TextureWrapMode)wrapmode, (FilterMode)filterMode);
            else
                prom = new AssetPromise_Texture(promiseURL);

            return prom;
        }

        [UnityTest]
        public IEnumerator FailCorrectlyWhenGivenWrongURL()
        {
            var prom = CreatePromise("123325");
            Asset_Texture asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            var prom2 = CreatePromise("43254378");
            Asset_Texture asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            var prom3 = CreatePromise("09898765");
            Asset_Texture asset3 = null;
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

        [UnityTest]
        public IEnumerator WaitForPromisesOfSameTextureWithDifferentSettings()
        {
            // default texture (no settings)
            var prom = CreatePromise(Utils.GetTestsAssetsPath() + "/Images/atlas.png");
            Asset_Texture asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };
            keeper.Keep(prom);

            // same texture but with settings
            var prom2 = CreatePromise(Utils.GetTestsAssetsPath() + "/Images/atlas.png", (int)TextureWrapMode.Repeat, (int)FilterMode.Trilinear);
            Asset_Texture asset2 = null;
            prom.OnSuccessEvent += (x) => { asset2 = x; };
            keeper.Keep(prom2);

            // different texture
            var prom3 = CreatePromise(Utils.GetTestsAssetsPath() + "/Images/avatar.png");
            Asset_Texture asset3 = null;
            prom.OnSuccessEvent += (x) => { asset3 = x; };
            keeper.Keep(prom3);

            Assert.AreEqual(AssetPromiseState.LOADING, prom.state);
            Assert.AreEqual(AssetPromiseState.WAITING, prom2.state);
            Assert.AreEqual(AssetPromiseState.LOADING, prom3.state);

            return null;
        }
    }
}
