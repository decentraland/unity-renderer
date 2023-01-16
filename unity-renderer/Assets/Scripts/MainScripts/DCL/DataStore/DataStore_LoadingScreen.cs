using System;

public class DataStore_LoadingScreen
{
    public readonly LoadingHUD loadingHUD = new ();
    public readonly DecoupledLoadingScreen decoupledLoadingHUD = new ();

    public class LoadingHUD
    {
        public readonly BaseVariable<bool> visible = new (false);
        public readonly BaseVariable<bool> fadeIn = new (false);
        public readonly BaseVariable<bool> fadeOut = new (false);
        public readonly BaseVariable<string> message = new (null);
        public readonly BaseVariable<float> percentage = new (0);
        public readonly BaseVariable<bool> showTips = new (false);
        public readonly BaseVariable<Exception> fatalError = new ();
    }

    public class DecoupledLoadingScreen
    {
        public readonly BaseVariable<bool> visible = new (false);
    }
}
