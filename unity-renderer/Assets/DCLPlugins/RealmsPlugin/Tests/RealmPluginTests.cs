using DCL;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmPlugin
{
    public class RealmPluginTests
    {
        private RealmPlugin realmPlugin;
        private const string CATALYST_REALM_NAME = "CatalystRealmName";
        private const string WORLD_REALM_NAME = "WorldRealmName";
        private const string ENABLE_GREEN_BLOCKERS_WORLDS_FF = "realms_blockers_in_worlds";
        private IRealmModifier genericModifier;
        private RealmBlockerModifier realmBlockerModiferSubstitute;
        private RealmMinimapModifier realmMinimapModiferSubstitute;

        [SetUp]
        public void SetUp()
        {
            realmPlugin = new RealmPlugin(DataStore.i);
            realmBlockerModiferSubstitute = Substitute.For<RealmBlockerModifier>(DataStore.i);

            realmMinimapModiferSubstitute = Substitute.For<RealmMinimapModifier>(DataStore.i);
            genericModifier = Substitute.For<IRealmModifier>();

            var substituteModifiers = new List<IRealmModifier>
                { realmBlockerModiferSubstitute, genericModifier, realmMinimapModiferSubstitute };

            realmPlugin.realmsModifiers = substituteModifiers;

            SetCatalystRealmsInfo();
        }

        [TearDown]
        public void TearDown()
        {
            realmPlugin.Dispose();
        }

        [TestCaseSource(nameof(GenericCases))]
        public void ModifierCalledOnRealmChange(string realmName, bool isCatalist)
        {
            // Act
            SetRealm(realmName);

            // Assert
            genericModifier.Received(1).OnEnteredRealm(isCatalist, Arg.Any<AboutResponse>());
        }

        [TestCaseSource(nameof(GreenBlockerCases))]
        public void GreenBlockerAddedOnRealmChange(string[] realmNames, int[] requiredLimit)
        {
            for (var i = 0; i < realmNames.Length; i++)
            {
                SetRealm(realmNames[i]);

                if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(ENABLE_GREEN_BLOCKERS_WORLDS_FF))
                    Assert.AreEqual(DataStore.i.worldBlockers.worldBlockerLimit.Get(), requiredLimit[i]);
            }
        }

        [TestCaseSource(nameof(GenericCases))]
        public void MinimapModifiedOnRealmChange(string realmName, bool shouldMinimapBeVisible)
        {
            SetRealm(realmName);
            Assert.AreEqual(DataStore.i.HUDs.minimapVisible.Get(), shouldMinimapBeVisible);
            Assert.AreEqual(DataStore.i.HUDs.jumpHomeButtonVisible.Get(), !shouldMinimapBeVisible);
        }

        private void SetRealm(string realmName)
        {
            DataStore.i.realm.playerRealmAbout.Set(new AboutResponse
            {
                Bff = new AboutResponse.Types.BffInfo(),
                Comms = new AboutResponse.Types.CommsInfo(),
                Configurations = new AboutResponse.Types.AboutConfiguration
                {
                    RealmName = realmName,
                    Minimap = new AboutResponse.Types.MinimapConfiguration
                    {
                        Enabled = realmName.Equals(CATALYST_REALM_NAME),
                    },
                },
            });
        }

        private void SetCatalystRealmsInfo()
        {
            var testRealmList = new List<RealmModel>();
            var testUsersCount = 100;

            testRealmList.Add(new RealmModel
            {
                serverName = CATALYST_REALM_NAME,
                layer = null,
                usersCount = testUsersCount,
            });

            DataStore.i.realm.realmsInfo.Set(testRealmList.ToArray());
        }

        private static object[] GreenBlockerCases =
        {
            new object[] { new[] { CATALYST_REALM_NAME, WORLD_REALM_NAME, CATALYST_REALM_NAME }, new[] { 0, 2, 0 } },
            new object[] { new[] { WORLD_REALM_NAME, CATALYST_REALM_NAME, WORLD_REALM_NAME }, new[] { 2, 0, 2 } },
        };

        private static object[] GenericCases =
        {
            new object[] { CATALYST_REALM_NAME, true },
            new object[] { WORLD_REALM_NAME, false },
        };
    }
}
