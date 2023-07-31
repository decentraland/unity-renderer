using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using UnityEngine;
using Environment = DCL.Environment;

namespace Features.CameraReel
{
    public class CameraReelPlugin: IPlugin
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
            var assetProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();

            var view = await CreateCameraReelSectionView(assetProvider);

            controller = new CameraReelSectionController(assetProvider, view, view.GalleryView, view.GalleryStorageView);
            controller.Initialize();
        }

        private static async UniTask<CameraReelSectionView> CreateCameraReelSectionView(IAddressableResourceProvider assetProvider) =>
            await assetProvider.Instantiate<CameraReelSectionView>(ADDRESS, ADDRESS,
                parent: DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.Get());

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
