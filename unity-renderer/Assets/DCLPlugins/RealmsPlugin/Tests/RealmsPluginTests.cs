using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmsPlugin
{
    public class RealmsPluginTests
    {
        private RealmsPlugin realmsPlugin;
        private const string catalystRealmName = "CatalystRealmName";
        private const string worldRealmName = "WorldRealmName";
        private IRealmsModifier genericModifier;
        private RealmsBlockerModifier realmsBlockerModiferSubstitute;
        private RealmsMinimapModifier realmsMinimapModiferSubstitute;
        private const string ENABLE_GREEN_BLOCKERS_WORLDS_FF = "realms_blockers_in_worlds";

        [SetUp]
        public void SetUp()
        {
            realmsPlugin = new RealmsPlugin(DataStore.i);
            realmsBlockerModiferSubstitute = Substitute.For<RealmsBlockerModifier>();

            realmsMinimapModiferSubstitute = Substitute.For<RealmsMinimapModifier>();
            genericModifier = Substitute.For<IRealmsModifier>();
            List<IRealmsModifier> substituteModifiers = new List<IRealmsModifier>() { realmsBlockerModiferSubstitute, genericModifier, realmsMinimapModiferSubstitute };
            realmsPlugin.realmsModifiers = substituteModifiers;

            SetCatalystRealmsInfo();
        }

        [TearDown]
        public void TearDown() { realmsPlugin.Dispose(); }

        [TestCaseSource(nameof(GenericCases))]
        public void ModifierCalledOnRealmChange(string realmName, bool isCatalist)
        {
            // Act
            SetRealm(realmName);

            // Assert
            genericModifier.Received(1).OnEnteredRealm(isCatalist, Arg.Any<AboutResponse >());
        }

        [TestCaseSource(nameof(GreenBlockerCases))]
        public void GreenBlockerAddedOnRealmChange(string[] realmNames, int[] requiredLimit)
        {
            for (int i = 0; i < realmNames.Length; i++)
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
                Configurations = new AboutResponse.Types.AboutConfiguration()
                {
                    RealmName = realmName,
                    Minimap = new AboutResponse.Types.MinimapConfiguration
                    {
                        Enabled = realmName.Equals(catalystRealmName)
                    }
                }
            });
        }

        private void SetCatalystRealmsInfo()
        {
            List<RealmModel> testRealmList = new List<RealmModel>();
            int testUsersCount = 100;
            testRealmList.Add(new RealmModel
            {
                serverName = catalystRealmName,
                layer = null,
                usersCount = testUsersCount
            });
            DataStore.i.realm.realmsInfo.Set(testRealmList.ToArray());
        }

        static object[] GreenBlockerCases =
        {
            new object[] { new [] { catalystRealmName, worldRealmName, catalystRealmName }, new [] { 0, 2, 0 } },
            new object[] { new [] { worldRealmName, catalystRealmName, worldRealmName }, new [] { 2, 0, 2 } }
        };

        static object[] GenericCases =
        {
            new object[] { catalystRealmName, true },
            new object[] { worldRealmName, false }
        };

    }
}
