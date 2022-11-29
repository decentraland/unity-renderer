using DCL;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCLPlugins.RealmPlugin
{
    public class RealmPluginTests
    {
        private RealmPlugin realmPlugin;
        private const string CATALYST_REALM_NAME = "CatalystRealmName";
        private const string WORLD_REALM_NAME = "WorldRealmName";
        private IRealmModifier genericModifier;

        [SetUp]
        public void SetUp()
        {
            realmPlugin = new RealmPlugin(DataStore.i);
            genericModifier = Substitute.For<IRealmModifier>();
            var substituteModifiers = new List<IRealmModifier>
                { genericModifier };
            realmPlugin.realmsModifiers = substituteModifiers;
        }

        [TearDown]
        public void TearDown() =>
            realmPlugin.Dispose();

        [TestCaseSource(nameof(isWorldCases))]
        public void ModifierCalledOnRealmChange(bool isWorld)
        {
            // Act
            SetRealm(isWorld);

            // Assert
            genericModifier.Received(1).OnEnteredRealm(isWorld, Arg.Any<AboutResponse.Types.AboutConfiguration>());
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
