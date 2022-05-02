using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Material_Tests
{
    public class APK_Material_Promise_Should : TestsBase_APK<AssetPromiseKeeper_Material,
        AssetPromise_Material,
        Asset_Material,
        AssetLibrary_RefCounted<Asset_Material>>
    {
        protected AssetPromise_Material CreatePromise()
        {
            TextureModel model = CreateTextureModel();
            var prom = new AssetPromise_Material(CreateMaterialModel(model));
            return prom;
        }

        protected AssetPromise_Material CreatePromise(TextureModel.BabylonWrapMode wrapmode, FilterMode filterMode)
        {
            TextureModel model = CreateTextureModel(wrapmode,filterMode);
            AssetPromise_Material prom = new AssetPromise_Material(CreateMaterialModel(model));

            return prom;
        }
        
        private MaterialModel CreateMaterialModel(TextureModel texture)
        {
            var newMaterialModel = new MaterialModel
            {
                albedoTexture = texture,
                metallic = 0,
                roughness = 1,
            };
            return newMaterialModel;
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

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            // Check non-default-settings texture
            Asset_Material loadedAsset = null;
            var prom = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.material);

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
            Asset_Material loadedAsset = null;
            var prom = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_Material loadedAsset2 = null;
            var prom2 = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.material);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.material);

            Assert.IsTrue(loadedAsset.material == loadedAsset2.material);

            // 2 default textures
            Asset_Material loadedAsset3 = null;
            var prom3 = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom3.OnSuccessEvent += (x) => loadedAsset3 = x;

            keeper.Keep(prom3);
            yield return prom3;

            Asset_Material loadedAsset4 = null;
            var prom4 = CreatePromise(TextureModel.BabylonWrapMode.WRAP, FilterMode.Trilinear);

            prom4.OnSuccessEvent += (x) => loadedAsset4 = x;

            keeper.Keep(prom4);
            yield return prom4;

            Assert.IsNotNull(loadedAsset3);
            Assert.IsNotNull(loadedAsset3.material);
            Assert.IsNotNull(loadedAsset4);
            Assert.IsNotNull(loadedAsset4.material);

            Assert.IsTrue(loadedAsset3.material == loadedAsset4.material);
        }
    }
}