using DCL.Map;

namespace DCL.GoToPanel
{
    public class GotoPanelPlugin : IPlugin
    {
        private readonly GotoPanelHUDController controller;

        public GotoPanelPlugin()
        {
            controller = new GotoPanelHUDController(GotoPanelHUDView.CreateView(),
                DataStore.i, Environment.i.serviceLocator.Get<ITeleportController>(),
                WebInterfaceMinimapApiBridge.i);
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
