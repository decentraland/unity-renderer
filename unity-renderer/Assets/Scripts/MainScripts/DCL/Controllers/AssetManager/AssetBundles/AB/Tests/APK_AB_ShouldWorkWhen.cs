using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_AssetBundle_Tests
{
    public class APK_AB_ShouldWorkWhen :
        APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_AB,
                                                AssetPromise_AB,
                                                Asset_AB,
                                                AssetLibrary_AB>
    {

        protected override AssetPromise_AB CreatePromise()
        {
            string contentUrl = TestAssetsUtils.GetPath() + "/AssetBundles/";
            string hash = "Qmc6ZNLGPoQaSrkE9qQ5sDm2dc9p8q9qsf5qYskvZCRskV";
            var prom = new AssetPromise_AB(contentUrl, hash);
            return prom;
        }
    }
}
