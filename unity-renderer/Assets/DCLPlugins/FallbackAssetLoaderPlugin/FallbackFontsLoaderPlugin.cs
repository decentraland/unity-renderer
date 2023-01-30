using DCL;
using System.Collections.Generic;
using TMPro;

namespace DCLPlugins.FallbackFontsLoader
{
    public class FallbackFontsLoaderPlugin : IPlugin
    {
        private const string FALLBACK_FONTS_ADDRESS = "Assets/Fonts/Fonts Sources/";
        private static readonly List<string> FALLBACK_FONTS_TO_LOAD = new () { "NotoSansJP-SemiBold SDF.asset", "NotoSansSC-SemiBold SDF.asset", "NotoSansKR-SemiBold SDF.asset" };

        private readonly List<AssetPromise_Font> assetPromiseFonts;

        public FallbackFontsLoaderPlugin()
        {
            assetPromiseFonts = new List<AssetPromise_Font>();

            foreach (string font in FALLBACK_FONTS_TO_LOAD)
            {
                var additionalFonts = new AssetPromise_Font(FALLBACK_FONTS_ADDRESS + font);
                additionalFonts.OnSuccessEvent += AddFallbackFont;
                AssetPromiseKeeper_Font.i.Keep(additionalFonts);
                assetPromiseFonts.Add(additionalFonts);
            }
        }

        public void Dispose() =>
            assetPromiseFonts.ForEach(e => AssetPromiseKeeper_Font.i?.Forget(e));

        private static void AddFallbackFont(Asset_Font obj)
        {
            List<TMP_FontAsset> fallbackFontAssets = TMP_Settings.fallbackFontAssets;

            if (fallbackFontAssets == null) { fallbackFontAssets = new List<TMP_FontAsset>(); }

            fallbackFontAssets.Add(obj.font);
            TMP_Settings.fallbackFontAssets = fallbackFontAssets;
        }
    }
}
