using DCL;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCLPlugins.RealmPlugin
{
    public class RealmMinimapModifierTests
    {
        private RealmPlugin realmPlugin;
        private const string CATALYST_REALM_NAME = "CatalystRealmName";
        private const string WORLD_REALM_NAME = "WorldRealmName";
        private IRealmModifier genericModifier;
        private RealmMinimapModifier realmMinimapModiferSubstitute;

        [SetUp]
        public void SetUp()
        {
            realmPlugin = new RealmPlugin(DataStore.i);
            realmMinimapModiferSubstitute = Substitute.For<RealmMinimapModifier>(DataStore.i);
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
            SetRealm(isWorld);

            // Assert
            Assert.AreEqual(DataStore.i.HUDs.minimapVisible.Get(), !isWorld);
            Assert.AreEqual(DataStore.i.HUDs.jumpHomeButtonVisible.Get(), isWorld);
        }

        private void SetRealm(bool isWorld)
        {
            List<string> sceneUrn = new List<string>();

            if (isWorld)
                sceneUrn.Add("sceneUrn");

            DataStore.i.realm.playerRealmAboutConfiguration.Set(new AboutResponse.Types.AboutConfiguration()
            {
                RealmName = isWorld ? WORLD_REALM_NAME : CATALYST_REALM_NAME,
                Minimap = new AboutResponse.Types.MinimapConfiguration()
                {
                    Enabled = !isWorld
                },
                ScenesUrn = { sceneUrn },
            });
        }

        private static bool[] isWorldCases = { false, true };
    }
}
