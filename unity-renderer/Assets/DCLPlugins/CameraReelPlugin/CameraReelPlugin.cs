using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLFeatures.CameraReel.ScreenshotViewer;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using UnityEngine;

namespace DCLPlugins.CameraReelPlugin
{
    public class CameraReelPlugin : IPlugin
    {
        private const string ADDRESS = "CameraReelSectionView";

        private Transform sectionParent;
        private CameraReelSectionController controller;

        public CameraReelPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            IAddressableResourceProvider assetProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();

            CameraReelSectionView view = await CreateCameraReelSectionView(assetProvider);
            CameraReelModel cameraReelModel = new ();
            ICameraReelGalleryService galleryService = Environment.i.serviceLocator.Get<ICameraReelGalleryService>();
            DataStore dataStore = DataStore.i;

            controller = new CameraReelSectionController(view, view.GalleryView, view.GalleryStorageView,
                dataStore,
                galleryService,
                cameraReelModel,
                () =>
                {
                    var screenshotViewerView = Object.Instantiate(view.ScreenshotViewerPrefab);
                    return new ScreenshotViewerController(screenshotViewerView, cameraReelModel, dataStore,
                        Environment.i.world.teleportController, galleryService);
                });

            dataStore.HUDs.isCameraReelInitialized.Set(true);
        }

        public void Dispose()
        {
            controller.Dispose();
        }

        private static async UniTask<CameraReelSectionView> CreateCameraReelSectionView(IAddressableResourceProvider assetProvider) =>
            await assetProvider.Instantiate<CameraReelSectionView>(ADDRESS, ADDRESS,
                parent: DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.Get());
    }
}
