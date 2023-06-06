using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using DCLPlugins.RealmsPlugin;
using UnityEngine;

namespace DCLPlugins.RealmPlugin
{
    public class RealmInfiniteFloorModifierTests
    {
        public RealmsInfiniteFloorModifier infiniteFloorModifier;

        private Texture mockMapTexture;
        private Texture mockEstateTexture;

        [SetUp]
        public void SetUp()
        {
            infiniteFloorModifier = new RealmsInfiniteFloorModifier(DataStore.i.HUDs);
            
            mockMapTexture = new Texture2D(100,100);
            mockEstateTexture = new Texture2D(100,100);
            DataStore.i.HUDs.latestDownloadedMainTexture.Set(mockMapTexture);
            DataStore.i.HUDs.latestDownloadedMapEstatesTexture.Set(mockEstateTexture);
        }

        [TearDown]
        public void TearDown() =>
            infiniteFloorModifier.Dispose();

        [TestCaseSource(nameof(isWorldCases))]
        public void InfiniteMapModifiedOnRealmChange(bool isWorld)
        {
            // Act
            infiniteFloorModifier.OnEnteredRealm(isWorld, null);

            // Assert
            Assert.AreEqual(DataStore.i.HUDs.mapMainTexture.Get() == null, isWorld);
            Assert.AreEqual(DataStore.i.HUDs.mapEstatesTexture.Get() == null, isWorld);
        }
        
        [Test]
        public void InfiniteMapModifiedAfterLeavingWorld()
        {
            // Act
            infiniteFloorModifier.OnEnteredRealm(true, null);
            Texture mockMapTexture_2 = new Texture2D(100,100);
            Texture mockEstateTexture_2 = new Texture2D(100,100);
            DataStore.i.HUDs.latestDownloadedMainTexture.Set(mockMapTexture_2);
            DataStore.i.HUDs.latestDownloadedMapEstatesTexture.Set(mockEstateTexture_2);
            
            // Assert
            Assert.IsNull(DataStore.i.HUDs.mapMainTexture.Get());
            Assert.IsNull(DataStore.i.HUDs.mapEstatesTexture.Get());
            
            // Act
            infiniteFloorModifier.OnEnteredRealm(false, null);

            // Assert
            Assert.AreEqual(DataStore.i.HUDs.mapMainTexture.Get(), mockMapTexture_2);
            Assert.AreEqual(DataStore.i.HUDs.mapEstatesTexture.Get(), mockEstateTexture_2);
        }
        
        private static bool[] isWorldCases = { false, true };
    }
}
