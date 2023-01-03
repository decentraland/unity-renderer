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
            loadingScreenController = new LoadingScreenController(LoadingScreenView.Create(), Environment.i.world.sceneController, DataStore.i.player,
                DataStore.i.common, dataStoreLoadingScreen.Ref,Environment.i.world.state);
        }

        public void Dispose() =>
            loadingScreenController.Dispose();
    }
}
