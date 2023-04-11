using DCLServices.WearablesCatalogService;

namespace DCL.Backpack
{
    public class BackpackEditorV2Plugin : IPlugin
    {
        private readonly BackpackEditorHUDController hudController;

        public BackpackEditorV2Plugin()
        {
            var view = BackpackEditorHUDV2ComponentView.Create();
            hudController = new BackpackEditorHUDController(
                view,
                DataStore.i,
                new UserProfileWebInterfaceBridge(),
                Environment.i.serviceLocator.Get<IEmotesCatalogService>());
            view.Initialize();
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
