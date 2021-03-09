using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Texture_Tests
{
    public class APK_Texture_Promise_Should : TestsBase_APK<AssetPromiseKeeper_Texture,
                                                            AssetPromise_Texture,
                                                            Asset_Texture,
                                                            AssetLibrary_Texture>
    {
        protected AssetPromise_Texture CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/Images/atlas.png";
            var prom = new AssetPromise_Texture(url);
            return prom;
        }

        protected AssetPromise_Texture CreatePromise(TextureWrapMode wrapmode, FilterMode filterMode)
        {
            string url = Utils.GetTestsAssetsPath() + "/Images/atlas.png";
            AssetPromise_Texture prom = new AssetPromise_Texture(url, wrapmode, filterMode);

            return prom;
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            // Check non-default-settings texture
            Asset_Texture loadedAsset = null;
            var prom = CreatePromise(TextureWrapMode.Repeat, FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture);
            Assert.AreEqual(loadedAsset.texture.wrapMode, TextureWrapMode.Repeat);
            Assert.AreEqual(loadedAsset.texture.filterMode, FilterMode.Trilinear);

            // Check default texture
            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture);

            TextureWrapMode defaultWrapMode = TextureWrapMode.Clamp;
            FilterMode defaultFilterMode = FilterMode.Bilinear;

            Assert.AreEqual(loadedAsset.texture.wrapMode, defaultWrapMode);
            Assert.AreEqual(loadedAsset.texture.filterMode, defaultFilterMode);
        }

        [UnityTest]
        public IEnumerator ShareTextureAmongPromisesWithSameSettings()
        {
            // 2 non-default textures
            Asset_Texture loadedAsset = null;
            var prom = CreatePromise(TextureWrapMode.Repeat, FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_Texture loadedAsset2 = null;
            var prom2 = CreatePromise(TextureWrapMode.Repeat, FilterMode.Trilinear);

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.texture);

            Assert.IsTrue(loadedAsset.texture == loadedAsset2.texture);

            // 2 default textures
            Asset_Texture loadedAsset3 = null;
            var prom3 = CreatePromise(TextureWrapMode.Repeat, FilterMode.Trilinear);

            prom3.OnSuccessEvent += (x) => loadedAsset3 = x;

            keeper.Keep(prom3);
            yield return prom3;

            Asset_Texture loadedAsset4 = null;
            var prom4 = CreatePromise(TextureWrapMode.Repeat, FilterMode.Trilinear);

            prom4.OnSuccessEvent += (x) => loadedAsset4 = x;

            keeper.Keep(prom4);
            yield return prom4;

            Assert.IsNotNull(loadedAsset3);
            Assert.IsNotNull(loadedAsset3.texture);
            Assert.IsNotNull(loadedAsset4);
            Assert.IsNotNull(loadedAsset4.texture);

            Assert.IsTrue(loadedAsset3.texture == loadedAsset4.texture);
        }
    }
}