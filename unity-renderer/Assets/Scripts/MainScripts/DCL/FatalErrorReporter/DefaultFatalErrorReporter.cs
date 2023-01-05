using System;

namespace DCL.FatalErrorReporter
{
    public class DefaultFatalErrorReporter : IFatalErrorReporter
    {
        private DataStoreRef<DataStore_LoadingScreen> loadingScreenRef;

        public void Report(Exception exception)
        {
            loadingScreenRef.Ref.loadingHUD.fatalError.Set(exception);
        }
    }
}
