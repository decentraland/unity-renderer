using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_Texture_Tests
{
    public class APK_PrimitiveTexture_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_PrimitiveMesh,
        AssetPromise_PrimitiveMesh,
        Asset_PrimitiveMesh,
        AssetLibrary_RefCounted<Asset_PrimitiveMesh>>
    {
        protected override AssetPromise_PrimitiveMesh CreatePromise()
        {
            PrimitiveMeshModel model = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Box);
            var prom = new AssetPromise_PrimitiveMesh(model);
            return prom;
        }
    }
}