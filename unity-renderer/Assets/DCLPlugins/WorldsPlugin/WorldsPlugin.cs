using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using Decentraland.Bff;
using UnityEngine;
using Variables.RealmsInfo;

public class WorldsPlugin : IPlugin
{

    internal List<IWorldsModifier> worldsModifiers;
    private BaseCollection<RealmModel> realmsList => DataStore.i.realm.realmsInfo;
    private BaseVariable<AboutResponse> realmAboutConfiguration => DataStore.i.realm.playerRealmAbout;
    private AboutResponse currentConfiguration;
    
    public WorldsPlugin()
    {
        worldsModifiers = new List<IWorldsModifier>() { new WorldsBlockerModifier() };
        realmAboutConfiguration.OnChange += RealmChanged;
        realmsList.OnSet += RealmListSet;
    }

    private void RealmListSet(IEnumerable<RealmModel> obj)
    {
        if (currentConfiguration != null)
            SetWorldModifiers();
    }

    private void RealmChanged(AboutResponse current, AboutResponse previous)
    {
        currentConfiguration = current;
        if (realmsList.Count().Equals(0))
            return;
        
        SetWorldModifiers();
    }

    private void SetWorldModifiers()
    {
        List<RealmModel> currentRealmsList = realmsList.Get().ToList();
        bool isCatalyst = currentRealmsList.FirstOrDefault(r => r.serverName == currentConfiguration.Configurations.RealmName) != null;
        worldsModifiers.ForEach(e => e.EnteredRealm(isCatalyst, currentConfiguration));
    }

    public void Dispose()
    {
        realmAboutConfiguration.OnChange -= RealmChanged;
        realmsList.OnSet -= RealmListSet;
        worldsModifiers.ForEach(e => e.Dispose());
    }
}