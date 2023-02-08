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
            AssetPromiseKeeper_Texture.i.library.Cleanup();
            return base.TearDown();
        }

        private AssetPromise_Material_Model CreateMaterialModel()
        {
            return AssetPromise_Material_Model.CreateBasicMaterial(new AssetPromise_Material_Model.Texture(
                TestAssetsUtils.GetPath() + "/Images/atlas.png", TextureWrapMode.Clamp, FilterMode.Bilinear), 1f, Color.white);
        }
    }
}