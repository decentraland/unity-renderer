using DCL;
using DCLPlugins.RealmsPlugin;
using Decentraland.Bff;

public class MinimapModifier : IRealmsModifier
{
    public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
    {
        DataStore.i.HUDs.minimapVisible.Set(isCatalyst || realmConfiguration.Configurations.Minimap.Enabled);
        DataStore.i.HUDs.jumpHomeButtonVisible.Set(!isCatalyst);
    }
    
    public void Dispose() { }

}