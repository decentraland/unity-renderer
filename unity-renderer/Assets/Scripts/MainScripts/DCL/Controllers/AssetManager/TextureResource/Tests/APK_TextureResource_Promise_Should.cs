using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_DCLTexture_Tests
{
    public class APK_TextureResource_Promise_Should : TestsBase_APK<AssetPromiseKeeper_TextureResource,
        AssetPromise_TextureResource,
        Asset_TextureResource,
        AssetLibrary_RefCounted<Asset_TextureResource>>
    {
        protected AssetPromise_TextureResource CreatePromise()
        {
            TextureModel model = CreateTextureModel();
            var prom = new AssetPromise_TextureResource(model);
            return prom;
        }

        protected AssetPromise_TextureResource CreatePromise(TextureModel.BabylonWrapMode wrapmode, FilterMode filterMode)
        {
            TextureModel model = CreateTextureModel(wrapmode,filterMode);
            AssetPromise_TextureResource prom = new AssetPromise_TextureResource(model);

            return prom;
        }

        protected TextureModel CreateTextureModel(TextureModel.BabylonWrapMode wrapmode = TextureModel.BabylonWrapMode.WRAP, FilterMode filterMode = FilterMode.Bilinear)
        {
            string url = TestAssetsUtils.GetPath() + "/Images/atlas.png";
            TextureModel model = new TextureModel();
            model.src = url;
            model.wrap = wrapmode;
            model.samplingMode = filterMode;
            return model;
        }
        
        protected override IEnumerator TearDown()
        {
            AssetPromiseKeeper_Texture.i.library.Cleanup();
            return base.TearDown();
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            // Check non-default-settings texture
            Asset_TextureResource loadedAsset = null;
            var prom = CreatePromise(TextureModel.BabylonWrapMode.CLAMP, FilterMode.Point);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture2D);
            Assert.AreEqual(loadedAsset.texture2D.wrapMode, TextureWrapMode.Clamp);
            Assert.AreEqual(loadedAsset.texture2D.filterMode, FilterMode.Point);

            // Check default texture
            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture2D);

            TextureWrapMode defaultWrapMode = TextureWrapMode.Repeat;
            FilterMode defaultFilterMode = FilterMode.Bilinear;

            Assert.AreEqual(loadedAsset.texture2D.wrapMode, defaultWrapMode);
            Assert.AreEqual(loadedAsset.texture2D.filterMode, defaultFilterMode);
        }

        [UnityTest]
        public IEnumerator ShareTextureAmongPromisesWithSameSettings()
        {
            // 2 non-default textures
            Asset_TextureResource loadedAsset = null;
            var prom = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_TextureResource loadedAsset2 = null;
            var prom2 = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture2D);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.texture2D);

            Assert.IsTrue(loadedAsset.texture2D == loadedAsset2.texture2D);

            // 2 default textures
            Asset_TextureResource loadedAsset3 = null;
            var prom3 = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom3.OnSuccessEvent += (x) => loadedAsset3 = x;

            keeper.Keep(prom3);
            yield return prom3;

            Asset_TextureResource loadedAsset4 = null;
            var prom4 = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom4.OnSuccessEvent += (x) => loadedAsset4 = x;

            keeper.Keep(prom4);
            yield return prom4;

            Assert.IsNotNull(loadedAsset3);
            Assert.IsNotNull(loadedAsset3.texture2D);
            Assert.IsNotNull(loadedAsset4);
            Assert.IsNotNull(loadedAsset4.texture2D);

            Assert.IsTrue(loadedAsset3.texture2D == loadedAsset4.texture2D);
        }

        [UnityTest]
        public IEnumerator FailWithBogusTexture()
        {
            string url = $"file://{Application.dataPath + "/../TestResources/Audio/Train.wav"}";

            Asset_TextureResource loadedAsset = null;
            var model = CreateTextureModel();
            model.src = url;
            AssetPromise_TextureResource prom = new AssetPromise_TextureResource(model);
            bool failed = false;
            bool succeeded = false;
            prom.OnSuccessEvent += (x) => succeeded = true;
            prom.OnFailEvent += (x, error) => failed = true;

            keeper.Keep(prom);
            yield return prom;

            Assert.IsTrue(failed);
            Assert.IsFalse(succeeded);
        }
    }
}