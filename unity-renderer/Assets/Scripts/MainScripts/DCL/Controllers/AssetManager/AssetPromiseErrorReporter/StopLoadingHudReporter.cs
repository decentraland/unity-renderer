using System;
using DCL;
using UnityEngine;

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
            Debug.LogException(exception);
            dataStore.HUDs.loadingHUD.error.Set(exception);
        }
    }
}