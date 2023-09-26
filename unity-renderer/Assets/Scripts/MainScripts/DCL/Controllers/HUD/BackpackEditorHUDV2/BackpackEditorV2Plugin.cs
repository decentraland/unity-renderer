using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Emotes;
using DCL.Providers;
using DCLServices.CustomNftCollection;
using DCLServices.DCLFileBrowser;
using DCLServices.Lambdas;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Components.Avatar.VRMExporter;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;

namespace DCL.Backpack
{
    public class BackpackEditorV2Plugin : IPlugin
    {
        private BackpackEditorHUDController hudController;

        public BackpackEditorV2Plugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            var assetsProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();
            var vrmExporterReferences = await assetsProvider.Instantiate<VRMExporterReferences>("VRMExporter", "_VRMExporter");

            ServiceLocator serviceLocator = Environment.i.serviceLocator;
            IWearablesCatalogService wearablesCatalogService = serviceLocator.Get<IWearablesCatalogService>();
            var userProfileBridge = new UserProfileWebInterfaceBridge();

            DataStore dataStore = DataStore.i;

            var view = BackpackEditorHUDV2ComponentView.Create();

            view.Initialize(
                serviceLocator.Get<ICharacterPreviewFactory>(),
                new PreviewCameraRotationController(),
                new PreviewCameraPanningController(),
                new PreviewCameraZoomController());

            var backpackAnalyticsService = new BackpackAnalyticsService(
                Environment.i.platform.serviceProviders.analytics,
                new NewUserExperienceAnalytics(Environment.i.platform.serviceProviders.analytics));

            view.OutfitsSectionComponentView.Initialize(serviceLocator.Get<ICharacterPreviewFactory>());

            var outfitsController = new OutfitsController(
                view.OutfitsSectionComponentView,
                new LambdaOutfitsService(
                    serviceLocator.Get<ILambdasService>(),
                    serviceLocator.Get<IServiceProviders>()),
                userProfileBridge,
                dataStore,
                backpackAnalyticsService);

            var backpackEmotesSectionController = new BackpackEmotesSectionController(
                dataStore,
                view.EmotesSectionTransform,
                userProfileBridge,
                serviceLocator.Get<IEmotesCatalogService>(),
                view.EmotesController,
                Environment.i.serviceLocator.Get<ICustomNftCollectionService>()
                );

            var backpackFiltersController = new BackpackFiltersController(view.BackpackFiltersComponentView, wearablesCatalogService);

            var avatarSlotsHUDController = new AvatarSlotsHUDController(view.AvatarSlotsView, backpackAnalyticsService, dataStore.featureFlags.flags);

            var wearableGridController = new WearableGridController(view.WearableGridComponentView,
                userProfileBridge,
                wearablesCatalogService,
                dataStore.backpackV2,
                new WebInterfaceBrowserBridge(),
                backpackFiltersController,
                avatarSlotsHUDController,
                backpackAnalyticsService,
                Environment.i.serviceLocator.Get<ICustomNftCollectionService>());

            hudController = new BackpackEditorHUDController(
                view,
                dataStore,
                CommonScriptableObjects.rendererState,
                userProfileBridge,
                wearablesCatalogService,
                backpackEmotesSectionController,
                backpackAnalyticsService,
                wearableGridController,
                avatarSlotsHUDController,
                outfitsController,
                new VRMExporter(vrmExporterReferences),
                Environment.i.platform.serviceLocator.Get<IDCLFileBrowserService>());

        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
