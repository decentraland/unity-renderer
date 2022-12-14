using DCL;
using static Decentraland.Bff.AboutResponse.Types;

namespace DCLPlugins.RealmPlugin
{
    /// <summary>
    /// Toggles the state of the minimap depending on the value of the AboutConfiguration.Minimap.Enabled we just entered.
    /// It also toggles the visibility of a Jump To Home button, which is required every time going into a world.
    /// If the minimap and the jump home button is visible, they can both co-exist on an ON state.
    /// </summary>
    public class RealmMinimapModifier : IRealmModifier
    {
        private readonly BaseVariable<bool> minimapVisible;
        private readonly BaseVariable<bool> jumpHomeButtonVisible;

        public RealmMinimapModifier(DataStore_HUDs dataStoreHUDs)
        {
            minimapVisible = dataStoreHUDs.minimapVisible;
            jumpHomeButtonVisible = dataStoreHUDs.jumpHomeButtonVisible;
        }

        public void Dispose() { }

        public void OnEnteredRealm(bool isWorld, AboutConfiguration realmConfiguration)
        {
            minimapVisible.Set(realmConfiguration.Minimap.Enabled);
            jumpHomeButtonVisible.Set(isWorld);
        }
    }
}
