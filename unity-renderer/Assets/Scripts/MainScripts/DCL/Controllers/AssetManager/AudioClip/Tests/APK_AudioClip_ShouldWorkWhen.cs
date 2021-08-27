using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using DCL.Tests;

namespace AssetPromiseKeeper_AudioClip_Tests
{
    public class APK_AudioClip_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_AudioClip,
        AssetPromise_AudioClip,
        Asset_AudioClip,
        AssetLibrary_RefCounted<Asset_AudioClip>>
    {

        protected override IEnumerator SetUp()
        {
            Environment.SetupWithBuilders(
                messagingBuilder: MessagingContextFactory.CreateMocked,
                platformBuilder: () => PlatformContextFactory.CreateWithGenericMocks(WebRequestController.Create()),
                worldRuntimeBuilder: WorldRuntimeContextFactory.CreateMocked,
                hudBuilder: HUDContextFactory.CreateDefault
            );

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