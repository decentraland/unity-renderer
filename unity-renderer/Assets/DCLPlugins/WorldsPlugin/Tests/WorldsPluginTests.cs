using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Variables.RealmsInfo;

namespace DCLPlugins.WorldsPlugin
{
    public class WorldsPluginTests
    {
        private WorldsPlugin worldsPlugin;
        private const string catalystRealmName = "CatalystRealmName";
        private const string worldRealmName = "WorldRealmName";
        private IWorldsModifier genericModifier;
        private WorldsBlockerModifier worldsBlockerModiferSubstitute;
        private ServiceLocator serviceLocator;

        [SetUp]
        public void SetUp()
        {
            // This is need to sue the TeleportController
            serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            Environment.Setup(serviceLocator);

            worldsPlugin = new WorldsPlugin();
            worldsBlockerModiferSubstitute = Substitute.For<WorldsBlockerModifier>();
            genericModifier = Substitute.For<IWorldsModifier>();
            List<IWorldsModifier> substituteModifiers = new List<IWorldsModifier>() { worldsBlockerModiferSubstitute, genericModifier };
            worldsPlugin.worldsModifiers = substituteModifiers;

            SetCatalystRealmsInfo();
        }
        
        [TearDown]
        public void TearDown() { worldsPlugin.Dispose(); }

        [TestCaseSource(nameof(GenericCases))]
        public void ModifierCalledOnRealmChange(string realmName, bool isCatalist)
        {
            // Act
            SetRealm(realmName);
        
            // Assert
            genericModifier.Received().OnEnteredRealm(isCatalist, Arg.Any<AboutResponse >());
        }
        
        [TestCaseSource(nameof(GreenBlockerCases))]
        public void GreenBlockerAddedOnRealmChange(string[] realmNames, bool[] isGreenBlockerEnabled)
        {
            for (int i = 0; i < realmNames.Length; i++)
            {
                SetRealm(realmNames[i]);
                serviceLocator.Get<IWorldBlockersController>().Received().SetEnabled(isGreenBlockerEnabled[i]);
            }
        }

        private void SetRealm(string realmName)
        {
            DataStore.i.realm.playerRealmAbout.Set(new AboutResponse
            {
                Bff = new AboutResponse.Types.BffInfo(),
                Comms = new AboutResponse.Types.CommsInfo(),
                Configurations = new AboutResponse.Types.AboutConfiguration()
                {
                    RealmName = realmName
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
            new object[] { new [] { catalystRealmName, worldRealmName, catalystRealmName }, new [] { true, false, true } },
            new object[] { new [] { worldRealmName, catalystRealmName, worldRealmName }, new [] { false, true, false } }
        };
        
        static object[] GenericCases =
        {
            new object[] { catalystRealmName, true },
            new object[] { worldRealmName, false }
        };
        
    }
}