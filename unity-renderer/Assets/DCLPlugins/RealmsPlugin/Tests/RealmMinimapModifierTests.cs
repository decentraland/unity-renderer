using DCL;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using WorldsFeaturesAnalytics;

namespace DCLPlugins.RealmPlugin
{
    public class RealmMinimapModifierTests
    {
        private RealmPlugin realmPlugin;
        private IRealmModifier genericModifier;
        private RealmMinimapModifier realmMinimapModiferSubstitute;
        private IWorldsAnalytics analytics;

        [SetUp]
        public void SetUp()
        {
            analytics = Substitute.For<IWorldsAnalytics>();

            realmPlugin = new RealmPlugin(DataStore.i, analytics);
            realmMinimapModiferSubstitute = Substitute.For<RealmMinimapModifier>(DataStore.i.HUDs);

            var substituteModifiers = new List<IRealmModifier>
                { realmMinimapModiferSubstitute };

            realmPlugin.realmsModifiers = substituteModifiers;
        }

        [TearDown]
        public void TearDown() =>
            realmPlugin.Dispose();

        [TestCaseSource(nameof(isWorldCases))]
        public void MinimapModifiedOnRealmChange(bool isWorld)
        {
            // Act
            RealmPluginTestsUtils.SetRealm(isWorld);

            // Assert
            Assert.AreEqual(DataStore.i.HUDs.minimapVisible.Get(), !isWorld);
            Assert.AreEqual(DataStore.i.HUDs.jumpHomeButtonVisible.Get(), isWorld);
        }

        private static bool[] isWorldCases = { false, true };
    }
}
