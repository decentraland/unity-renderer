using AssetPromiseKeeper_Tests;
using DCL;
using MainScripts.DCL.Controllers.AssetManager;

namespace AssetPromiseKeeper_Font_Tests
{
    public class APK_Font_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_Font,
        AssetPromise_Font,
        Asset_Font,
        AssetLibrary_RefCounted<Asset_Font>>
    {
        private const string fontName = "SansSerif";

        protected override AssetPromise_Font CreatePromise()
        {
            var prom = new AssetPromise_Font(fontName, AssetSource.EMBEDDED);
            return prom;
        }
    }
}
