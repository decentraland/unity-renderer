using System;

namespace DCL.FatalErrorReporter
{
    public class DefaultFatalErrorReporter : IFatalErrorReporter
    {
        private DataStoreRef<DataStore_LoadingScreen> loadingScreenRef;

        public void Report(Exception exception)
        {
            //TODO: Should we still report fatal error on loading screen?
        }
    }
}
