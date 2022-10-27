using System.Collections.Generic;
using System.Linq;
using DCL;
using Variables.RealmsInfo;

public class WorldsPlugin : IPlugin
{

    private List<IWorldsModifier> worldsModifiers;
    private BaseCollection<RealmModel> realmsList => DataStore.i.realm.realmsInfo; 
    
    public WorldsPlugin()
    {
        worldsModifiers = new List<IWorldsModifier> { new WorldsBlockerModifier() };
        DataStore.i.realm.playerRealm.OnChange += RealmChanged;
    }
    
    private void RealmChanged(CurrentRealmModel current, CurrentRealmModel previous)
    {
        List<RealmModel> currentRealmsList = realmsList.Get().ToList();
        if (currentRealmsList?.Count() == 0)
            return;
            
        RealmModel currentRealmModel = currentRealmsList.FirstOrDefault(r => r.serverName == current.serverName);
        if (currentRealmModel == null)
            worldsModifiers.ForEach(e => e.EnteredWorld());
        else
            worldsModifiers.ForEach(e => e.ExitedWorld());
    }

    public void Dispose()
    {
        DataStore.i.realm.playerRealm.OnChange -= RealmChanged;
    }
}
