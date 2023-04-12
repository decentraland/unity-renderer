using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;

namespace DCL.Backpack
{
    public class BackpackEditorV2Plugin : IPlugin
    {
        private readonly BackpackEditorHUDController hudController;

        public BackpackEditorV2Plugin()
        {
            var view = BackpackEditorHUDV2ComponentView.Create();
            view.Initialize(Environment.i.serviceLocator.Get<ICharacterPreviewFactory>());

            hudController = new BackpackEditorHUDController(
                view,
                DataStore.i,
                new UserProfileWebInterfaceBridge(),
                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                Environment.i.serviceLocator.Get<IEmotesCatalogService>());
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
