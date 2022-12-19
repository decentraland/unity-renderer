using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCLPlugins.RealmPlugin
{
    public class RealmMinimapModifierTests
    {
        private RealmPlugin realmPlugin;
        private IRealmModifier genericModifier;
        private RealmMinimapModifier realmMinimapModiferSubstitute;

        [SetUp]
        public void SetUp()
        {
            realmPlugin = new RealmPlugin(DataStore.i);
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
