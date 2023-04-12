using DCLServices.WearablesCatalogService;

namespace DCL.Backpack
{
    public class BackpackEditorV2Plugin : IPlugin
    {
        private readonly BackpackEditorHUDController hudController;

        public BackpackEditorV2Plugin()
        {
            hudController = new BackpackEditorHUDController(
                BackpackEditorHUDV2ComponentView.Create(),
                DataStore.i,
                new UserProfileWebInterfaceBridge(),
                Environment.i.serviceLocator.Get<IEmotesCatalogService>());
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
