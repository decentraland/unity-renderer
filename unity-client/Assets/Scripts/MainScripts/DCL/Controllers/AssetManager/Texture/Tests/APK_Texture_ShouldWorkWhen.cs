using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_Texture_Tests
{
    public class APK_Texture_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_Texture,
                                                                                        AssetPromise_Texture,
                                                                                        Asset_Texture,
                                                                                        AssetLibrary_Texture>
    {
        protected override AssetPromise_Texture CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/Images/atlas.png";
            var prom = new AssetPromise_Texture(url);
            return prom;
        }
    }
}
