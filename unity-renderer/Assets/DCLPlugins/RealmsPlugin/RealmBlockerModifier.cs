using DCL;
using Decentraland.Bff;
using UnityEngine;

namespace DCLPlugins.RealmPlugin
{
    public class RealmBlockerModifier : IRealmModifier
    {

        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            bool shouldGreenBlockersBeActive = isCatalyst || ShouldGreenBlockersBeActive(realmConfiguration.Configurations.CityLoaderContentServer);
            if (shouldGreenBlockersBeActive)
                DataStore.i.worldBlockers.worldBlockerLimits.Set(0);
            else
                DataStore.i.worldBlockers.worldBlockerLimits.Set(2);
        }

        public void Dispose() { }

        private bool ShouldGreenBlockersBeActive(string cityLoaderContentServers)
            => !string.IsNullOrEmpty(cityLoaderContentServers);
    }
}