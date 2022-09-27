
namespace DCL
{
    public class ABDetectorPlugin : IPlugin
    {
        private readonly ABDetectorTracker abDetectorTracker;
        
        public ABDetectorPlugin()
        {
            var dataStore = DataStore.i;
            var worldState = Environment.i.world.state;
            abDetectorTracker = new ABDetectorTracker(dataStore.debugConfig, dataStore.player, worldState);
        }
        
        public void Dispose()
        {
            abDetectorTracker?.Dispose();
        }
    } 
}
