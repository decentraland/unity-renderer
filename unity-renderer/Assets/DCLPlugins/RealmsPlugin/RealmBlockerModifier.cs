using DCL;
using Decentraland.Bff;
using JetBrains.Annotations;

namespace DCLPlugins.RealmPlugin
{
    public class RealmBlockerModifier : IRealmModifier
    {

        private const int WORLD_BLOCKER_LIMIT = 2;
        [CanBeNull] private const string ENABLE_GREEN_BLOCKERS_WORLDS_FF = "realms_blockers_in_worlds";

        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            bool shouldGreenBlockersBeActive = isCatalyst || ShouldGreenBlockersBeActive(realmConfiguration.Configurations.CityLoaderContentServer);
            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(ENABLE_GREEN_BLOCKERS_WORLDS_FF))
                DataStore.i.worldBlockers.worldBlockerLimit.Set(shouldGreenBlockersBeActive ? 0 : WORLD_BLOCKER_LIMIT);
            else
                DataStore.i.worldBlockers.worldBlockerEnabled.Set(shouldGreenBlockersBeActive);
        }

        public void Dispose() { }

        private bool ShouldGreenBlockersBeActive(string cityLoaderContentServers)
            => !string.IsNullOrEmpty(cityLoaderContentServers);
    }
}
