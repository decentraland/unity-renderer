using DCL;
using Decentraland.Bff;
using UnityEngine;

namespace DCLPlugins.RealmsPlugin
{
    public class RealmsBlockerModifier : IRealmsModifier
    {
        
        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        { 
            Debug.Log("CALLING ON ENTEREDREALM OF REALMSBLOCKERMODIFIER " + isCatalyst + " " + ShouldGreenBlockersBeActive(realmConfiguration.Configurations.CityLoaderContentServer));
            Environment.i.world.blockersController.SetEnabled(isCatalyst || ShouldGreenBlockersBeActive(realmConfiguration.Configurations.CityLoaderContentServer));
        }
     
        public void Dispose() { }

        private bool ShouldGreenBlockersBeActive(string cityLoaderContentServers)
        {
            return !string.IsNullOrEmpty(cityLoaderContentServers);
        }
        
    }
}