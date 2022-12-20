using DCL;
using DCL.LoadingScreen;

namespace DCLPlugins.LoadingScreenPlugin
{
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
