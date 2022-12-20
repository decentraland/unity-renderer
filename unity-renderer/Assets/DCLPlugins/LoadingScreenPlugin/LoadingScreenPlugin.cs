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

        public LoadingScreenPlugin()
        {
            loadingScreenController = new LoadingScreenController(LoadingScreenView.Create(),  Environment.i.world.sceneController);
        }

        public void Dispose()
        {
            loadingScreenController.Dispose();
        }
    }
}
