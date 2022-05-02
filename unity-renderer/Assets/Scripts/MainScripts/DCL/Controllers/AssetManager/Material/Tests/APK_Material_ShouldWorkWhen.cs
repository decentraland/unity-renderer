using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using UnityEngine;

namespace AssetPromiseKeeper_Material_Tests
{
    public class APK_Material_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_Material,
        AssetPromise_Material,
        Asset_Material,
        AssetLibrary_RefCounted<Asset_Material>>
    {
        protected override AssetPromise_Material CreatePromise()
        {
            var model = CreateMaterialModel();
            var prom = new AssetPromise_Material(model);
            return prom;
        }
        
        protected override IEnumerator TearDown()
        {
            AssetPromiseKeeper_Texture.i.library.Cleanup();
            AssetPromiseKeeper_TextureResource.i.library.Cleanup();
            return base.TearDown();
        }
        
        private MaterialModel CreateMaterialModel()
        {
            var texture = CreateTextureModel();
            
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
    }
}