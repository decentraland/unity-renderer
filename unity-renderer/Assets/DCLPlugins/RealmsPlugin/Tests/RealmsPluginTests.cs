using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmPlugin
{
    public class RealmsPluginTests
    {
        private RealmPlugin realmPlugin;
        private const string catalystRealmName = "CatalystRealmName";
        private const string worldRealmName = "WorldRealmName";
        private IRealmModifier genericModifier;
        private RealmBlockerModifier realmBlockerModiferSubstitute;
        private RealmMinimapModifier realmMinimapModiferSubstitute;

        private ServiceLocator serviceLocator;

        [SetUp]
        public void SetUp()
        {
            realmPlugin = new RealmPlugin();
            realmBlockerModiferSubstitute = Substitute.For<RealmBlockerModifier>();

            realmMinimapModiferSubstitute = Substitute.For<RealmMinimapModifier>();
            genericModifier = Substitute.For<IRealmModifier>();
            List<IRealmModifier> substituteModifiers = new List<IRealmModifier>() { realmBlockerModiferSubstitute, genericModifier, realmMinimapModiferSubstitute };
            realmPlugin.realmsModifiers = substituteModifiers;

            SetCatalystRealmsInfo();
        }
        
        [TearDown]
        public void TearDown() { realmPlugin.Dispose(); }

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
                Assert.AreEqual(DataStore.i.worldBlockers.worldBlockerLimits.Get(), requiredLimit[i]);
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