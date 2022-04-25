using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Material_Tests
{
    public class BlockedAndMasterPromisesShould : TestsBase_APK<AssetPromiseKeeper_Material,
        AssetPromise_Material,
        Asset_Material,
        AssetLibrary_RefCounted<Asset_Material>>
    {
        protected override IEnumerator TearDown()
        {
            AssetPromiseKeeper_Texture.i.library.Cleanup();
            AssetPromiseKeeper_DCLTexture.i.library.Cleanup();
            return base.TearDown();
        }
        
        protected AssetPromise_Material CreatePromise(string srcTexture)
        {
            AssetPromise_Material prom;
            var  model = CreateMaterialModel(srcTexture);
            
            prom = new AssetPromise_Material(model);

            return prom;
        }

        private MaterialModel CreateMaterialModel(string srcTexture)
        {
            var texture = new TextureModel
            {
                src =srcTexture,
                wrap = TextureModel.BabylonWrapMode.WRAP,
                samplingMode = FilterMode.Bilinear,
            };
            
            var newMaterialModel = new MaterialModel
            {
                albedoTexture = texture,
                metallic = 0,
                roughness = 1,
            };
            return newMaterialModel;
        }

        [UnityTest]
        public IEnumerator WaitForPromisesOfSameTextureWithDifferentSettings()
        {
            // default texture (no settings)
            var prom = CreatePromise(TestAssetsUtils.GetPath() + "/Images/atlas.png");
            Asset_Material asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };
            keeper.Keep(prom);

            // same texture but with settings
            var prom2 = CreatePromise(TestAssetsUtils.GetPath() + "/Images/atlas.png");
            Asset_Material asset2 = null;
            prom.OnSuccessEvent += (x) => { asset2 = x; };
            keeper.Keep(prom2);

            // different texture
            var prom3 = CreatePromise(TestAssetsUtils.GetPath() + "/Images/avatar.png");
            Asset_Material asset3 = null;
            prom.OnSuccessEvent += (x) => { asset3 = x; };
            keeper.Keep(prom3);

            Assert.AreEqual(AssetPromiseState.LOADING, prom.state);
            Assert.AreEqual(AssetPromiseState.WAITING, prom2.state);
            Assert.AreEqual(AssetPromiseState.LOADING, prom3.state);

            return null;
        }
    }
}