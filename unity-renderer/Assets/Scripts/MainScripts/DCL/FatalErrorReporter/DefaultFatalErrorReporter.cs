using System;

namespace DCL.FatalErrorReporter
{
    public class DefaultFatalErrorReporter : IFatalErrorReporter
    {
        private readonly DataStoreRef<DataStore_LoadingScreen> dataStore_LoadingScreen;

        public DefaultFatalErrorReporter()
        {

        }

        public void Report(Exception exception)
        {
            dataStore_LoadingScreen.Ref.loadingHUD.fatalError.Set(exception);
        }
    }
}
