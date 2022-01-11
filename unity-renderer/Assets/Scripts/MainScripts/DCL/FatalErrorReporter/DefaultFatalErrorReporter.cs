using System;

namespace DCL.FatalErrorReporter
{
    public class DefaultFatalErrorReporter : IFatalErrorReporter
    {
        private readonly DataStore dataStore;

        public DefaultFatalErrorReporter(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }
        
        public void Report(Exception exception)
        {
            dataStore.HUDs.loadingHUD.fatalError.Set(exception);
        }
    }
}