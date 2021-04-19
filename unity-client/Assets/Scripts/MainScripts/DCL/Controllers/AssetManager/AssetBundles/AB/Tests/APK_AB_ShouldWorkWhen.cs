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
            string contentUrl = Utils.GetTestsAssetsPath() + "/AssetBundles/";
            string hash = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
            var prom = new AssetPromise_AB(contentUrl, hash);
            return prom;
        }
    }
}
