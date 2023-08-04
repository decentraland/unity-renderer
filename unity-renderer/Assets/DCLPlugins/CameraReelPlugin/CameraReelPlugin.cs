using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLFeatures.CameraReel.Section;
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
            controller = new CameraReelSectionController(view, view.GalleryView, view.GalleryStorageView);
            controller.Initialize();

            DataStore.i.HUDs.isCameraReelInitialized.Set(true);
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
