
namespace DCL
{
    public class ABDetectorPlugin : IPlugin
    {
        private readonly ABDetectorTracker abDetectorTracker;
        
        public ABDetectorPlugin()
        {
            abDetectorTracker = new ABDetectorTracker(DataStore.i.debugConfig);
        }
        
        public void Dispose()
        {
            abDetectorTracker?.Dispose();
        }
    } 
}
