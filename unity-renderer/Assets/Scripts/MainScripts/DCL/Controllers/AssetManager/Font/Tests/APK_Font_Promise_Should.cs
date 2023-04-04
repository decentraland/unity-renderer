using AssetPromiseKeeper_Tests;
using DCL;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Font_Tests
{
    public class APK_Font_Promise_Should : TestsBase_APK<AssetPromiseKeeper_Font,
        AssetPromise_Font,
        Asset_Font,
        AssetLibrary_RefCounted<Asset_Font>>
    {
        private const string fontName = "SansSerif";

        protected AssetPromise_Font CreatePromise()
        {
            var prom = new AssetPromise_Font(fontName);
            return prom;
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            // Check non-default-settings texture
            Asset_Font loadedAsset = null;
            AssetPromise_Font prom = CreatePromise();

            prom.OnSuccessEvent += x => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.font);

            // Check default texture
            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += x => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
        }

        [UnityTest]
        public IEnumerator ShareTextureAmongPromisesWithSameSettings()
        {
            // 2 non-default textures
            Asset_Font loadedAsset = null;
            AssetPromise_Font prom = CreatePromise();

            prom.OnSuccessEvent += x => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_Font loadedAsset2 = null;
            AssetPromise_Font prom2 = CreatePromise();

            prom2.OnSuccessEvent += x => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.font);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.font);

            Assert.IsTrue(loadedAsset.font == loadedAsset2.font);
        }

        [UnityTest]
        public IEnumerator KeepRefCountCorrectly()
        {
            string model = fontName;
            var prom = new AssetPromise_Font(model);
            keeper.Keep(prom);
            yield return prom;

            Assert.AreEqual(1, keeper.library.masterAssets[model].referenceCount);

            var prom2 = new AssetPromise_Font(model);
            keeper.Keep(prom2);
            yield return prom2;

            Assert.AreEqual(2, keeper.library.masterAssets[model].referenceCount);

            keeper.Forget(prom);
            Assert.AreEqual(1, keeper.library.masterAssets[model].referenceCount);

            prom = new AssetPromise_Font(model);
            keeper.Keep(prom);
            yield return prom;

            Assert.AreEqual(2, keeper.library.masterAssets[model].referenceCount);
            keeper.Forget(prom);
            keeper.Forget(prom2);

            Assert.AreEqual(0, keeper.library.masterAssets.Count);
        }
    }
}
