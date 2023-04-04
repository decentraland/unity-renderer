using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_AudioClip_Tests
{
    public class APK_AudioClip_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_AudioClip,
        AssetPromise_AudioClip,
        Asset_AudioClip,
        AssetLibrary_RefCounted<Asset_AudioClip>>
    {
        protected override IEnumerator SetUp()
        {
            ServiceLocator serviceLocator = DCL.ServiceLocatorTestFactory.CreateMocked();

            //Setting the AssetResolverLogger.VERBOSE_LOG_FLAG to false since the singleton crashes a couple of subsequent test.
            DataStore.i.featureFlags.flags.Set(new FeatureFlag { } );

            serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
            Environment.Setup(serviceLocator);
            return base.SetUp();
        }

        protected override IEnumerator TearDown()
        {
            Environment.Dispose();
            return base.TearDown();
        }

        protected override AssetPromise_AudioClip CreatePromise()
        {
            string url = TestAssetsUtils.GetPath() + "/Audio/short_effect.ogg";
            var prom = new AssetPromise_AudioClip(url, new ContentProvider_Dummy());
            return prom;
        }
    }
}
