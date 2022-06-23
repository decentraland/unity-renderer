using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
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

        protected AssetPromise_Font CreatePromise(string name)
        {
            AssetPromise_Font prom = new AssetPromise_Font(name);
            return prom;
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            // Check non-default-settings texture
            Asset_Font loadedAsset = null;
            var prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.font);

            // Check default texture
            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
        }

        [UnityTest]
        public IEnumerator ShareTextureAmongPromisesWithSameSettings()
        {
            // 2 non-default textures
            Asset_Font loadedAsset = null;
            var prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_Font loadedAsset2 = null;
            var prom2 = CreatePromise();

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.font);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.font);

            Assert.IsTrue(loadedAsset.font == loadedAsset2.font);
        }
    }
}