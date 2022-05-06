using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using UnityEngine;

namespace AssetPromiseKeeper_DCLTexture_Tests
{
    public class APK_TextureResource_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_TextureResource,
        AssetPromise_TextureResource,
        Asset_TextureResource,
        AssetLibrary_RefCounted<Asset_TextureResource>>
    {
        protected override AssetPromise_TextureResource CreatePromise()
        {
            var model = CreateTextureModel();
            var prom = new AssetPromise_TextureResource(model);
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