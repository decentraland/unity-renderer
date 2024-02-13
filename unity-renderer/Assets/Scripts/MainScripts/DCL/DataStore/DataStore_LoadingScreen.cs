using System;

public class DataStore_LoadingScreen
{
    public readonly DecoupledLoadingScreen decoupledLoadingHUD = new ();

    public class DecoupledLoadingScreen
    {
        public readonly BaseVariable<bool> visible = new (false);
        public BaseVariable<bool> loadingScreenV2Enabled = new (false);
    }
}
