using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_Gif_Tests
{
    public class APK_Gif_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_Gif,
                                                                                        AssetPromise_Gif,
                                                                                        Asset_Gif,
                                                                                        AssetLibrary_RefCounted<Asset_Gif>>
    {
        protected override AssetPromise_Gif CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/Images/gif1.gif";
            var prom = new AssetPromise_Gif(url);
            return prom;
        }
    }
}
