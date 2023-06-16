using DCL.Browser;
using DCLServices.Lambdas;
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
            view.Initialize(
            Environment.i.serviceLocator.Get<ICharacterPreviewFactory>(),
            new PreviewCameraRotationController(),
            new PreviewCameraPanningController(),
            new PreviewCameraZoomController());

            DataStore dataStore = DataStore.i;

            view.OutfitsSectionComponentView.Initialize(Environment.i.serviceLocator.Get<ICharacterPreviewFactory>());

            var outfitsController = new OutfitsController(
                view.OutfitsSectionComponentView,
                new LambdaOutfitsService(
                    Environment.i.serviceLocator.Get<ILambdasService>(),
                    Environment.i.serviceLocator.Get<IServiceProviders>()),
                userProfileBridge,
                wearablesCatalogService);

            var backpackEmotesSectionController = new BackpackEmotesSectionController(
                dataStore,
                view.EmotesSectionTransform,
                userProfileBridge,
                Environment.i.serviceLocator.Get<IEmotesCatalogService>());

            var backpackAnalyticsService = new BackpackAnalyticsService(
                Environment.i.platform.serviceProviders.analytics,
                new NewUserExperienceAnalytics(Environment.i.platform.serviceProviders.analytics));

            var backpackFiltersController = new BackpackFiltersController(view.BackpackFiltersComponentView, wearablesCatalogService);

            var avatarSlotsHUDController = new AvatarSlotsHUDController(view.AvatarSlotsView, backpackAnalyticsService);

            var wearableGridController = new WearableGridController(view.WearableGridComponentView,
                userProfileBridge,
                wearablesCatalogService,
                dataStore.backpackV2,
                new WebInterfaceBrowserBridge(),
                backpackFiltersController,
                avatarSlotsHUDController,
                backpackAnalyticsService);

            hudController = new BackpackEditorHUDController(
                view,
                dataStore,
                CommonScriptableObjects.rendererState,
                userProfileBridge,
                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                backpackEmotesSectionController,
                backpackAnalyticsService,
                wearableGridController,
                avatarSlotsHUDController,
                outfitsController);
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
