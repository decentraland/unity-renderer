using DCL.Browser;
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

            DataStore dataStore = DataStore.i;

            var backpackEmotesSectionController = new BackpackEmotesSectionController(
                dataStore,
                view.EmotesSectionTransform,
                userProfileBridge,
                Environment.i.serviceLocator.Get<IEmotesCatalogService>());

            var backpackAnalyticsController = new BackpackAnalyticsController(
                Environment.i.platform.serviceProviders.analytics,
                new NewUserExperienceAnalytics(Environment.i.platform.serviceProviders.analytics),
                wearablesCatalogService);

            var backpackFiltersController = new BackpackFiltersController(view.BackpackFiltersComponentView, wearablesCatalogService);

            var avatarSlotsHUDController = new AvatarSlotsHUDController(view.AvatarSlotsView);

            var wearableGridController = new WearableGridController(view.WearableGridComponentView,
                userProfileBridge,
                wearablesCatalogService,
                dataStore.backpackV2,
                new WebInterfaceBrowserBridge(),
                backpackFiltersController,
                avatarSlotsHUDController,
                backpackAnalyticsController);

            hudController = new BackpackEditorHUDController(
                view,
                dataStore,
                CommonScriptableObjects.rendererState,
                userProfileBridge,
                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                backpackEmotesSectionController,
                backpackAnalyticsController,
                wearableGridController,
                avatarSlotsHUDController);
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
