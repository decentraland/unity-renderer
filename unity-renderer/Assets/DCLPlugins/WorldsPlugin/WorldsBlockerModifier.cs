using DCL;
using Decentraland.Bff;

namespace DCLPlugins.WorldsPlugin
{
    public class WorldsBlockerModifier : IWorldsModifier
    {
        
        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration) =>
            Environment.i.world.blockersController.SetEnabled(isCatalyst || ShouldGreenBlockersBeActive(realmConfiguration.Configurations.CityLoaderContentServer));    
     
        public void Dispose() { }

        private bool ShouldGreenBlockersBeActive(string cityLoaderContentServers)
        {
            return !string.IsNullOrEmpty(cityLoaderContentServers);
        }
        
    }
}