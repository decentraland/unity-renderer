using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Variables.RealmsInfo;

public class WorldsPluginTests
{

    private WorldsPlugin worldsPlugin;
    private string catalystRealmName = "CatalystRealmName";
    private string worldRealmName = "WorldRealmName";
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
    
    [Test]
    public void ModifierCalledOnRealmChange()
    {
        SetCatalystRealm();
        genericModifier.Received().EnteredRealm(true, Arg.Any<AboutResponse >());

        SetWorldsRealm();
        genericModifier.Received().EnteredRealm(false, Arg.Any<AboutResponse >());
    }

    [Test]
    public void GreenBlockerAddedOnRealmChange()
    {
        SetCatalystRealm();
        serviceLocator.Get<IWorldBlockersController>().Received().SetEnabled(true);

        SetWorldsRealm();
        serviceLocator.Get<IWorldBlockersController>().Received().SetEnabled(false);
    }

    private void SetCatalystRealm()
    {
        DataStore.i.realm.playerRealmAbout.Set(new AboutResponse
        {
            Bff = new AboutResponse.Types.BffInfo(),
            Comms = new AboutResponse.Types.CommsInfo(),
            Configurations = new AboutResponse.Types.AboutConfiguration()
            {
                RealmName = catalystRealmName
            }
        });
    }
    
    private void SetWorldsRealm()
    {
        DataStore.i.realm.playerRealmAbout.Set(new AboutResponse
        {
            Bff = new AboutResponse.Types.BffInfo(),
            Comms = new AboutResponse.Types.CommsInfo(),
            Configurations = new AboutResponse.Types.AboutConfiguration()
            {
                RealmName = worldRealmName
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
    

    [TearDown]
    public void TearDown()
    {
        worldsPlugin.Dispose();
    }
}
