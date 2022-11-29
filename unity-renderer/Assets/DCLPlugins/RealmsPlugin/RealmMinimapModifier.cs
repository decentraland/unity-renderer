using DCL;
using Decentraland.Bff;

namespace DCLPlugins.RealmPlugin
{
    public class RealmMinimapModifier : IRealmModifier
    {
        private readonly BaseVariable<bool> minimapVisible;
        private readonly BaseVariable<bool> jumpHomeButtonVisible;

        public RealmMinimapModifier(DataStore dataStore)
        {
            minimapVisible = dataStore.HUDs.minimapVisible;
            jumpHomeButtonVisible = dataStore.HUDs.jumpHomeButtonVisible;
        }

        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            minimapVisible.Set(realmConfiguration.Configurations.Minimap.Enabled);
            jumpHomeButtonVisible.Set(!isCatalyst);
        }

        public void Dispose() { }
    }
}
