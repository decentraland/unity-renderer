using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;

namespace DCL.Backpack
{
    public class BackpackEditorV2Plugin : IPlugin
    {
        private readonly BackpackEditorHUDController hudController;

        public BackpackEditorV2Plugin()
        {
            IWearablesCatalogService wearablesCatalogService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();
            var userProfileBridge = new UserProfileWebInterfaceBridge();

            var view = BackpackEditorHUDV2ComponentView.Create();
            view.Initialize(Environment.i.serviceLocator.Get<ICharacterPreviewFactory>());

            var backpackEmotesSectionController = new BackpackEmotesSectionController(
                DataStore.i,
                view.EmotesSectionTransform,
                userProfileBridge,
                Environment.i.serviceLocator.Get<IEmotesCatalogService>());

            var backpackAnalyticsController = new BackpackAnalyticsController(
                Environment.i.platform.serviceProviders.analytics,
                new NewUserExperienceAnalytics(Environment.i.platform.serviceProviders.analytics),
                wearablesCatalogService);

            var wearableGridController = new WearableGridController(view.WearableGridComponentView, userProfileBridge, wearablesCatalogService);

            hudController = new BackpackEditorHUDController(
                view,
                DataStore.i,
                CommonScriptableObjects.rendererState,
                userProfileBridge,
                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                backpackEmotesSectionController,
                backpackAnalyticsController,
                wearableGridController);
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
