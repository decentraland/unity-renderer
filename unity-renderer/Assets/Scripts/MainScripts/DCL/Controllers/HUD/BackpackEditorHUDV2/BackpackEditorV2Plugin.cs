using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Providers;
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

            IWearablesCatalogService wearablesCatalogService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();
            var userProfileBridge = new UserProfileWebInterfaceBridge();

            DataStore dataStore = DataStore.i;

            var view = BackpackEditorHUDV2ComponentView.Create();

            view.Initialize(
                Environment.i.serviceLocator.Get<ICharacterPreviewFactory>(),
                new PreviewCameraRotationController(),
                new PreviewCameraPanningController(),
                new PreviewCameraZoomController());

            var backpackAnalyticsService = new BackpackAnalyticsService(
                Environment.i.platform.serviceProviders.analytics,
                new NewUserExperienceAnalytics(Environment.i.platform.serviceProviders.analytics));

            view.OutfitsSectionComponentView.Initialize(Environment.i.serviceLocator.Get<ICharacterPreviewFactory>());

            var outfitsController = new OutfitsController(
                view.OutfitsSectionComponentView,
                new LambdaOutfitsService(
                    Environment.i.serviceLocator.Get<ILambdasService>(),
                    Environment.i.serviceLocator.Get<IServiceProviders>()),
                userProfileBridge,
                dataStore,
                backpackAnalyticsService);

            var backpackEmotesSectionController = new BackpackEmotesSectionController(
                dataStore,
                view.EmotesSectionTransform,
                userProfileBridge,
                Environment.i.serviceLocator.Get<IEmotesCatalogService>());

            var backpackFiltersController = new BackpackFiltersController(view.BackpackFiltersComponentView, wearablesCatalogService);

            var avatarSlotsHUDController = new AvatarSlotsHUDController(view.AvatarSlotsView, backpackAnalyticsService, dataStore.featureFlags.flags);

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
