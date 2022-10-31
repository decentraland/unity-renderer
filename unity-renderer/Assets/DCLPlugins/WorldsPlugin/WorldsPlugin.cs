using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using UnityEngine;
using Variables.RealmsInfo;

public class WorldsPlugin : IPlugin
{

    private List<IWorldsModifier> worldsModifiers;
    private BaseCollection<RealmModel> realmsList => DataStore.i.realm.realmsInfo; 
    
    public WorldsPlugin()
    {
        worldsModifiers = new List<IWorldsModifier> { new WorldsBlockerModifier() };
        DataStore.i.realm.realmInfo.OnChange += RealmChanged;
    }
    private void RealmChanged(string current, string previous)
    {
        AboutResponse_AboutConfiguration newConfiguration = JsonUtility.FromJson<AboutResponse_AboutConfiguration>(current);

        List<RealmModel> currentRealmsList = realmsList.Get().ToList();
        RealmModel currentRealmModel = currentRealmsList.FirstOrDefault(r => r.serverName == newConfiguration.realmName);
        
        //worldsModifiers.ForEach(e => e.EnteredRealm((currentRealmsList?.Count() == 0 || currentRealmModel != null), newConfiguration));
    }

    public void Dispose()
    {
        DataStore.i.realm.realmInfo.OnChange -= RealmChanged;
    }
}

[Serializable]
public class AboutResponse_AboutConfiguration
{
    public string realmName;
    public int networkId;
    public string[] globalScenesUrn;
    public string[] scenesUrn;
    public AboutResponse_MinimapConfiguration minimap;
    public AboutResponse_SkyboxConfiguration fixedHour;
    public string cityLoaderContentServer;
}

[Serializable]
public class AboutResponse_MinimapConfiguration
{
    public bool enabled;
    public string dataImage;
    public string estateImage;
}

[Serializable]
public class AboutResponse_SkyboxConfiguration {
    public int fixedHour;
}