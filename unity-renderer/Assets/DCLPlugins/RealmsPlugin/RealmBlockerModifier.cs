using DCL;
using static Decentraland.Bff.AboutResponse.Types;

namespace DCLPlugins.RealmPlugin
{
    /// <summary>
    /// Toggles the state of the green blockers depending if the realm just entered is a world.
    /// Also, it has a feature flag so we have the possibility to turn them off entirely.
    /// </summary>
    public class RealmBlockerModifier : IRealmModifier
    {
        private const int WORLD_BLOCKER_LIMIT = 2;
        private const string ENABLE_GREEN_BLOCKERS_WORLDS_FF = "realms_blockers_in_worlds";

        private readonly BaseVariable<bool> worldBlockersEnabled;
        private readonly BaseVariable<int> worldBlockersLimit;

        public RealmBlockerModifier(DataStore_WorldBlockers dataStoreWorldBlockers)
        {
            worldBlockersEnabled = dataStoreWorldBlockers.worldBlockerEnabled;
            worldBlockersLimit = dataStoreWorldBlockers.worldBlockerLimit;
        }

        public void Dispose() { }

        public void OnEnteredRealm(bool isWorld, AboutConfiguration realmConfiguration)
        {
            bool shouldGreenBlockersBeActive = !isWorld || HasContentServers(realmConfiguration.CityLoaderContentServer);

            bool HasContentServers(string cityLoaderContentServers) =>
                !string.IsNullOrEmpty(cityLoaderContentServers);

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(ENABLE_GREEN_BLOCKERS_WORLDS_FF))
                worldBlockersLimit.Set(shouldGreenBlockersBeActive ? 0 : WORLD_BLOCKER_LIMIT);
            else
                worldBlockersEnabled.Set(shouldGreenBlockersBeActive);
        }
    }
}
