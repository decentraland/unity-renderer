using System;
using DCL;

namespace AssetPromiseErrorReporter
{
    public class StopLoadingHudReporter : IAssetPromiseErrorReporter
    {
        private readonly DataStore dataStore;

        public StopLoadingHudReporter(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }
        
        public void Report(Exception exception)
        {
            dataStore.HUDs.loadingHUD.error.Set(exception);
        }
    }
}