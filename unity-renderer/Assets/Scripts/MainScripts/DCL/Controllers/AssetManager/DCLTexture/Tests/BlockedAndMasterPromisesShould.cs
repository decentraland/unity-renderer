using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_DCLTexture_Tests
{
    public class BlockedAndMasterPromisesShould : TestsBase_APK<AssetPromiseKeeper_DCLTexture,
        AssetPromise_DCLTexture,
        Asset_DCLTexture,
        AssetLibrary_RefCounted<Asset_DCLTexture>>
    {
        protected override IEnumerator TearDown()
        {
            AssetPromiseKeeper_Texture.i.library.Cleanup();
            return base.TearDown();
        }
        
        protected AssetPromise_DCLTexture CreatePromise(string promiseURL, int wrapmode = -1, int filterMode = -1)
        {
            AssetPromise_DCLTexture prom;

            TextureModel model = new TextureModel();
            model.src = promiseURL;
            if (filterMode > -1 && wrapmode > -1)
            {
                model.wrap = (TextureModel.BabylonWrapMode)wrapmode;
                model.samplingMode = (FilterMode)filterMode;
            }

            prom = new AssetPromise_DCLTexture(model);

            return prom;
        }

        [UnityTest]
        public IEnumerator Texture_FailCorrectlyWhenGivenWrongURL()
        {
            var prom = CreatePromise("123325");
            Asset_DCLTexture asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x, error) => { failEventCalled1 = true; };

            var prom2 = CreatePromise("43254378");
            Asset_DCLTexture asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x, error) => { failEventCalled2 = true; };

            var prom3 = CreatePromise("09898765");
            Asset_DCLTexture asset3 = null;
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