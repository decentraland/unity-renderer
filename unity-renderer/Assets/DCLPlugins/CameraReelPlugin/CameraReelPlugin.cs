using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Providers;
using DCLFeatures.CameraReel.Gallery;
using DCLFeatures.CameraReel.ScreenshotViewer;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using System.Collections.Generic;
using UnityEngine;

namespace DCLPlugins.CameraReelPlugin
{
    public class CameraReelPlugin : IPlugin
    {
        private const string ADDRESS = "CameraReelSectionView";

        private readonly List<ThumbnailContextMenuController> thumbnailContextMenuControllers = new ();

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
            ICameraReelGalleryService galleryService = Environment.i.serviceLocator.Get<ICameraReelGalleryService>();
            DataStore dataStore = DataStore.i;
            CameraReelModel cameraReelModel = CameraReelModel.i;

            controller = new CameraReelSectionController(view, view.GalleryView, view.GalleryStorageView,
                dataStore,
                galleryService,
                cameraReelModel,
                () =>
                {
                    var screenshotViewerView = Object.Instantiate(view.ScreenshotViewerPrefab);
                    return new ScreenshotViewerController(screenshotViewerView, cameraReelModel, dataStore,
                        galleryService);
                });

            ThumbnailContextMenuView.Instances.OnAdded += OnThumbnailContextMenuAdded;

            dataStore.HUDs.isCameraReelInitialized.Set(true);
        }

        public void Dispose()
        {
            ThumbnailContextMenuView.Instances.OnAdded -= OnThumbnailContextMenuAdded;

            foreach (ThumbnailContextMenuController controller in thumbnailContextMenuControllers)
                controller.Dispose();

            this.controller.Dispose();
        }

        private void OnThumbnailContextMenuAdded(ThumbnailContextMenuView view)
        {
            ThumbnailContextMenuController controller = new (view, Clipboard.Create(), CameraReelModel.i,
                new WebInterfaceBrowserBridge(),
                Environment.i.serviceLocator.Get<ICameraReelGalleryService>(),
                DataStore.i);
            thumbnailContextMenuControllers.Add(controller);
        }

        private static async UniTask<CameraReelSectionView> CreateCameraReelSectionView(IAddressableResourceProvider assetProvider) =>
            await assetProvider.Instantiate<CameraReelSectionView>(ADDRESS, ADDRESS,
                parent: DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.Get());
    }
}
