using System;

namespace DCL.FatalErrorReporter
{
    public class DefaultFatalErrorReporter : IFatalErrorReporter
    {
        private readonly DataStore_LoadingScreen dataStore;

        public DefaultFatalErrorReporter(DataStore_LoadingScreen dataStore)
        {
            this.dataStore = dataStore;
        }

        public void Report(Exception exception)
        {
            dataStore.loadingHUD.fatalError.Set(exception);
        }
    }
}
