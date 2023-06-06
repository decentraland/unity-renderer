using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCLPlugins.RealmPlugin
{
    public class RealmMinimapModifierTests
    {
        private IRealmModifier genericModifier;
        private RealmMinimapModifier realmMinimapModifier;

        [SetUp]
        public void SetUp()
        {
            realmMinimapModifier = new RealmMinimapModifier(DataStore.i.HUDs);
        }

        [TearDown]
        public void TearDown() =>
            realmMinimapModifier.Dispose();

        [TestCaseSource(nameof(isWorldCases))]
        public void MinimapModifiedOnRealmChange(bool isWorld)
        {
            // Act
            realmMinimapModifier.OnEnteredRealm(isWorld, RealmPluginTestsUtils.GetAboutConfiguration(isWorld));

            // Assert
            Assert.AreEqual(DataStore.i.HUDs.minimapVisible.Get(), !isWorld);
            Assert.AreEqual(DataStore.i.HUDs.jumpHomeButtonVisible.Get(), isWorld);
        }

        private static bool[] isWorldCases = { false, true };
    }
}
