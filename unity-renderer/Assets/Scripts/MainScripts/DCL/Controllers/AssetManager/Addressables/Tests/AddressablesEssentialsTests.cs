using DCL.Emotes;
using DCL.Providers;
using DCL.Skybox;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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

            //Assert
            Assert.NotNull(asset);
            //We check that the 31 embedded emotes are present
            Assert.AreEqual(asset.emotes.Count(), 31);
            //We validate just the first emote, wave
            Assert.AreEqual(asset.emotes[0].id, "wave");
            Assert.NotNull(asset.emotes[0].femaleAnimation);
            Assert.NotNull(asset.emotes[0].maleAnimation);
        }

        [Test]
        public async Task EnsureEssentials()
        {
            // Arrange
            var addressableProvider = new AddressableResourceProvider();

            //Act
            var loadingScreen = await addressableProvider.GetAddressable<GameObject>("_LoadingScreen");

            //Assert
            Assert.NotNull(loadingScreen);
        }

        [Test]
        public async Task EnsureSkyboxEssentials()
        {
            // Arrange
            var addressableProvider = new AddressableResourceProvider();

            //Act
            var skyboxConfiguration = await addressableProvider.GetAddressable<SkyboxConfiguration>("Generic_Skybox.asset");
            var skyboxElements = await addressableProvider.GetAddressable<GameObject>("SkyboxElements.prefab");
            var skyboxPrefab = await addressableProvider.GetAddressable<GameObject>("SkyboxProbe.prefab");
            var materialReferenceContainer = await addressableProvider.GetAddressable<MaterialReferenceContainer>("SkyboxMaterialData.asset");

            //Assert
            Assert.NotNull(skyboxConfiguration);
            Assert.NotNull(skyboxElements);
            Assert.NotNull(skyboxPrefab);
            Assert.NotNull(materialReferenceContainer);
        }
    }
}

