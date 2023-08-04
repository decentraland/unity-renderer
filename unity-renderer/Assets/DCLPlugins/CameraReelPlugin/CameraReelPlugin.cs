using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Providers;
using DCLFeatures.CameraReel.Gallery;
using DCLFeatures.CameraReel.ScreenshotViewer;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
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

        public CameraReelPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            IAddressableResourceProvider assetProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();

            CameraReelSectionView view = await CreateCameraReelSectionView(assetProvider);
            ICameraReelService service = Environment.i.serviceLocator.Get<ICameraReelService>();
            DataStore dataStore = DataStore.i;
            CameraReelModel cameraReelModel = CameraReelModel.i;

            reelSectionController = new CameraReelSectionController(view, view.GalleryView, view.GalleryStorageView,
                dataStore,
                service,
                cameraReelModel,
                () =>
                {
                    var screenshotViewerView = Object.Instantiate(view.ScreenshotViewerPrefab);
                    return new ScreenshotViewerController(screenshotViewerView, cameraReelModel, dataStore,
                        service, new UserProfileWebInterfaceBridge());
                });

            ThumbnailContextMenuView.Instances.OnAdded += OnThumbnailContextMenuAdded;
            ScreenshotVisiblePersonView.Instances.OnAdded += OnVisiblePersonAdded;

            dataStore.HUDs.isCameraReelInitialized.Set(true);
        }

        public void Dispose()
        {
            ThumbnailContextMenuView.Instances.OnAdded -= OnThumbnailContextMenuAdded;
            ScreenshotVisiblePersonView.Instances.OnAdded -= OnVisiblePersonAdded;

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
            ThumbnailContextMenuController controller = new (view, Clipboard.Create(), CameraReelModel.i,
                new WebInterfaceBrowserBridge(),
                Environment.i.serviceLocator.Get<ICameraReelService>(),
                DataStore.i);
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
                DataStore.i));
        }
    }
}
