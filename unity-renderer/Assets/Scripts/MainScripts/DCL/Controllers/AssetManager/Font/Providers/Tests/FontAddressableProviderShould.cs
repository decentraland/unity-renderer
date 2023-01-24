using NUnit.Framework;
using System;
using System.Threading.Tasks;
using TMPro;

namespace DCL.Tests
{
    [TestFixture]
    public class FontAddressableProviderShould
    {

        [Test]
        public async Task RetrievedCorrectlyAnAddressableFont()
        {
            var fontName = "NotoSansJP-SemiBold SDF";
            var fallbackFontToLoad = $"Assets/Fonts/Fonts Sources/{fontName}.asset";

            var addressableFontProvider = new AddressableFontProvider();

            TMP_FontAsset asset = await addressableFontProvider.GetFontAsync(fallbackFontToLoad);

            Assert.NotNull(asset);
            Assert.AreEqual(fontName, asset.name);
        }

    }
}
