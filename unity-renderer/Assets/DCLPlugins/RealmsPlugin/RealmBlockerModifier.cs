using DCL;
using Decentraland.Bff;

namespace DCLPlugins.RealmPlugin
{
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

        public void OnEnteredRealm(bool isWorld, AboutResponse.Types.AboutConfiguration realmConfiguration)
        {
            bool shouldGreenBlockersBeActive = !isWorld || ShouldGreenBlockersBeActive(realmConfiguration.CityLoaderContentServer);

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(ENABLE_GREEN_BLOCKERS_WORLDS_FF))
                worldBlockersLimit.Set(shouldGreenBlockersBeActive ? 0 : WORLD_BLOCKER_LIMIT);
            else
                worldBlockersEnabled.Set(shouldGreenBlockersBeActive);
        }

        public void Dispose() { }

        private bool ShouldGreenBlockersBeActive(string cityLoaderContentServers) =>
            !string.IsNullOrEmpty(cityLoaderContentServers);
    }
}
