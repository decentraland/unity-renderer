using DCL.Emotes;
using DCL.Providers;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace DCL.Tests
{
    [TestFixture]
    public class AddressablesEssentialsTests
    {
        [Test]
        public async Task EnsureEmbeddedEmotesEssentials()
        {
            // Arrange
            var addressableProvider = new AddressableResourceProvider();

            //Act
            EmbeddedEmotesSO asset = await addressableProvider.GetAddressable<EmbeddedEmotesSO>("EmbeddedEmotes.asset");

            Assert.NotNull(asset);
            //We check that the 31 embedded emotes are present
            Assert.AreEqual(asset.emotes.Count(), 31);
            //We validate just the first emote, wave
            Assert.AreEqual(asset.emotes[0].id, "wave");
            Assert.NotNull(asset.emotes[0].femaleAnimation);
            Assert.NotNull(asset.emotes[0].maleAnimation);
        }
    }
}

