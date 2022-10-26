using System.Collections.Generic;
using System.Linq;
using DCL;
using Variables.RealmsInfo;

public class WorldsPlugin : IPlugin
{


    public WorldsPlugin()
    {
        DataStore.i.realm.playerRealm.OnChange += RealmChanged;
    }
    
    private void RealmChanged(CurrentRealmModel current, CurrentRealmModel previous)
    {
        List<RealmModel> realmList = DataStore.i.realm.realmsInfo.Get()?.ToList();
        if (realmList?.Count == 0)
            return;
            
        RealmModel currentRealmModel = realmList?.FirstOrDefault(r => r.serverName == current.serverName);
        if (currentRealmModel == null)
        {
            EnteredWorld();
        }
        else
        {
            ExitedWorld();
        }
    }
    private void ExitedWorld()
    {
        Environment.i.world.blockersController.SetEnabled(true);

    }
    private void EnteredWorld()
    {
        Environment.i.world.blockersController.SetEnabled(false);
    }

    public void Dispose()
    {
        DataStore.i.realm.playerRealm.OnChange -= RealmChanged;
    }
}
