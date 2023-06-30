using DCL;
using DCL.LoadingScreen.V2;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenV2Plugin : IPlugin
{
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

    public LoadingScreenV2Plugin()
    {
        dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.Set(true);
    }

    public void Dispose()
    {
    }
}
