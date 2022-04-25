using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using UnityEngine;

namespace AssetPromiseKeeper_DCLTexture_Tests
{
    public class APK_DCLTexture_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_DCLTexture,
        AssetPromise_DCLTexture,
        Asset_DCLTexture,
        AssetLibrary_RefCounted<Asset_DCLTexture>>
    {
        protected override AssetPromise_DCLTexture CreatePromise()
        {
            var model = CreateTextureModel();
            var prom = new AssetPromise_DCLTexture(model);
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
    }
}