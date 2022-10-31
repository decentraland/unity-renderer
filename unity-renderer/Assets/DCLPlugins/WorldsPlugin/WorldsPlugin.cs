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
    private BaseVariable<AboutResponse.Types.AboutConfiguration> realmAboutConfiguration => DataStore.i.realm.realmAboutConfiguration;
    private AboutResponse.Types.AboutConfiguration currentConfiguration;
    public WorldsPlugin()
    {
        realmAboutConfiguration.OnChange += RealmChanged;
        realmsList.OnSet += RealmListSet;
    }

    private void RealmListSet(IEnumerable<RealmModel> obj)
    {
        if (currentConfiguration != null)
            SetWorldModifiers();
    }

    private void RealmChanged(AboutResponse.Types.AboutConfiguration current, AboutResponse.Types.AboutConfiguration previous)
    {
        if (realmsList.Count().Equals(0))
            return;

        currentConfiguration = current;
        SetWorldModifiers();
    }

    private void SetWorldModifiers()
    {
        List<RealmModel> currentRealmsList = realmsList.Get().ToList();
        bool isRegularRealm = currentRealmsList.FirstOrDefault(r => r.serverName == currentConfiguration.RealmName) != null;
        worldsModifiers.ForEach(e => e.EnteredRealm(isRegularRealm, currentConfiguration));
    }

    public void Dispose()
    {
        realmAboutConfiguration.OnChange -= RealmChanged;
        realmsList.OnSet -= RealmListSet;
        worldsModifiers.ForEach(e => e.Dispose());
    }
}