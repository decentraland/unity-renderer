using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_AssetBundle_GameObject_Tests
{
    public class APK_AB_GameObject_ShouldWorkWhen :
        APKWithPoolableAssetShouldWorkWhen_Base<AssetPromiseKeeper_AB_GameObject,
                                                AssetPromise_AB_GameObject,
                                                Asset_AB_GameObject,
                                                AssetLibrary_AB_GameObject>
    {

        protected override AssetPromise_AB_GameObject CreatePromise()
        {
            string contentUrl = Utils.GetTestsAssetsPath() + "/AssetBundles/";
            string hash = "QmNS4K7GaH63T9rhAfkrra7ADLXSEeco8FTGknkPnAVmKM";
            var prom = new AssetPromise_AB_GameObject(contentUrl, hash);
            return prom;
        }
    }
}
