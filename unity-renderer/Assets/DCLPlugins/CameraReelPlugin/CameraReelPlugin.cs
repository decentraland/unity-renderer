using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Providers;
using DCLFeatures.CameraReel.Gallery;
using DCLFeatures.CameraReel.ScreenshotViewer;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using DCLServices.EnvironmentProvider;
using DCLServices.WearablesCatalogService;
using System.Collections.Generic;
using UnityEngine;

namespace DCLPlugins.CameraReelPlugin
{
    public class CameraReelPlugin : IPlugin
    {
        private const string ADDRESS = "CameraReelSectionView";

        private readonly List<ThumbnailContextMenuController> thumbnailContextMenuControllers = new ();
        private readonly List<ScreenshotVisiblePersonController> visiblePersonControllers = new ();

        private Transform sectionParent;
        private CameraReelSectionController reelSectionController;
        private CameraReelModel cameraReelModel;

        private ICameraReelStorageService storageService;

        public CameraReelPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            await UniTask.WaitUntil(() => DataStore.i.player.ownPlayer.Get() != null && !string.IsNullOrEmpty(DataStore.i.player.ownPlayer.Get().id));

            if (UserProfileController.userProfilesCatalog.Get(DataStore.i.player.ownPlayer.Get().id).isGuest)
                return;

            IAddressableResourceProvider assetProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();

            CameraReelSectionView view = await CreateCameraReelSectionView(assetProvider);
            storageService = Environment.i.serviceLocator.Get<ICameraReelStorageService>();
            DataStore dataStore = DataStore.i;
            cameraReelModel = new CameraReelModel();
            ICameraReelAnalyticsService analytics = Environment.i.serviceLocator.Get<ICameraReelAnalyticsService>();

            reelSectionController = new CameraReelSectionController(view, view.GalleryView, view.GalleryStorageView,
                dataStore,
                storageService,
                cameraReelModel,
                () =>
                {
                    ScreenshotViewerView screenshotViewerView = Object.Instantiate(view.ScreenshotViewerPrefab);

                    return new ScreenshotViewerController(screenshotViewerView, cameraReelModel, dataStore,
                        storageService, new UserProfileWebInterfaceBridge(),
                        Clipboard.Create(), new WebInterfaceBrowserBridge(),
                        analytics, Environment.i.serviceLocator.Get<IEnvironmentProviderService>(),
                        screenshotViewerView.ActionPanel,
                        screenshotViewerView.InfoSidePanel);
                }, analytics);

            ThumbnailContextMenuView.Instances.OnAdded += OnThumbnailContextMenuAdded;
            ScreenshotVisiblePersonView.Instances.OnAdded += OnVisiblePersonAdded;

            storageService.ScreenshotUploaded += cameraReelModel.AddScreenshotAsFirst;

            dataStore.HUDs.isCameraReelInitialized.Set(true);
        }

        public void Dispose()
        {
            ThumbnailContextMenuView.Instances.OnAdded -= OnThumbnailContextMenuAdded;
            ScreenshotVisiblePersonView.Instances.OnAdded -= OnVisiblePersonAdded;

            storageService.ScreenshotUploaded -= cameraReelModel.AddScreenshotAsFirst;

            foreach (ThumbnailContextMenuController controller in thumbnailContextMenuControllers)
                controller.Dispose();

            thumbnailContextMenuControllers.Clear();

            foreach (ScreenshotVisiblePersonController controller in visiblePersonControllers)
                controller.Dispose();

            visiblePersonControllers.Clear();

            reelSectionController.Dispose();
        }

        private void OnThumbnailContextMenuAdded(ThumbnailContextMenuView view)
        {
            ThumbnailContextMenuController controller = new (view, Clipboard.Create(), cameraReelModel,
                new WebInterfaceBrowserBridge(),
                Environment.i.serviceLocator.Get<ICameraReelStorageService>(),
                DataStore.i,
                Environment.i.serviceLocator.Get<ICameraReelAnalyticsService>(),
                Environment.i.serviceLocator.Get<IEnvironmentProviderService>());

            thumbnailContextMenuControllers.Add(controller);
        }

        private static async UniTask<CameraReelSectionView> CreateCameraReelSectionView(IAddressableResourceProvider assetProvider) =>
            await assetProvider.Instantiate<CameraReelSectionView>(ADDRESS, ADDRESS,
                parent: DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.Get());

        private void OnVisiblePersonAdded(ScreenshotVisiblePersonView view)
        {
            visiblePersonControllers.Add(new ScreenshotVisiblePersonController(view,
                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                new UserProfileWebInterfaceBridge(),
                new WebInterfaceBrowserBridge(),
                DataStore.i,
                Environment.i.serviceLocator.Get<ICameraReelAnalyticsService>()));
        }
    }
}
