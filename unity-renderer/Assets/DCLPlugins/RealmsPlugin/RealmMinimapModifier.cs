using DCL;
using Decentraland.Bff;

namespace DCLPlugins.RealmPlugin
{
    public class RealmMinimapModifier : IRealmModifier
    {
        private readonly BaseVariable<bool> minimapVisible;
        private readonly BaseVariable<bool> jumpHomeButtonVisible;

        public RealmMinimapModifier(DataStore_HUDs dataStoreHUDs)
        {
            minimapVisible = dataStoreHUDs.minimapVisible;
            jumpHomeButtonVisible = dataStoreHUDs.jumpHomeButtonVisible;
        }

        public void OnEnteredRealm(bool isWorld, AboutResponse.Types.AboutConfiguration realmConfiguration)
        {
            minimapVisible.Set(realmConfiguration.Minimap.Enabled);
            jumpHomeButtonVisible.Set(isWorld);
        }

        public void Dispose() { }
    }
}
