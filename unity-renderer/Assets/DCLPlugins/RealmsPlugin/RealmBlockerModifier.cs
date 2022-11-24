using DCL;
using Decentraland.Bff;
using UnityEngine;

namespace DCLPlugins.RealmPlugin
{
    public class RealmBlockerModifier : IRealmModifier
    {

        private const int WORLD_BLOCKER_LIMIT = 2;
        
        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            bool shouldGreenBlockersBeActive = isCatalyst || ShouldGreenBlockersBeActive(realmConfiguration.Configurations.CityLoaderContentServer);
            DataStore.i.worldBlockers.worldBlockerLimits.Set(shouldGreenBlockersBeActive ? 0 : WORLD_BLOCKER_LIMIT);
        }

        public void Dispose() { }

        private bool ShouldGreenBlockersBeActive(string cityLoaderContentServers)
            => !string.IsNullOrEmpty(cityLoaderContentServers);
    }
}