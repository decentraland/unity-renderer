using DCL;
using DCL.LoadingScreen;

namespace DCLPlugins.LoadingScreenPlugin
{
    /// <summary>
    /// Plugin controller for the decoupled Loading Screen
    /// </summary>
    public class LoadingScreenPlugin : IPlugin
    {
        private readonly LoadingScreenController loadingScreenController;
        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

        public LoadingScreenPlugin()
        {
            dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.Set(true);
            loadingScreenController = new LoadingScreenController(LoadingScreenView.Create(), Environment.i.world.sceneController, Environment.i.world.state, NotificationsController.i,DataStore.i.player,
                DataStore.i.common, dataStoreLoadingScreen.Ref, DataStore.i.realm);
        }

        public void Dispose() =>
            loadingScreenController.Dispose();
    }
}
