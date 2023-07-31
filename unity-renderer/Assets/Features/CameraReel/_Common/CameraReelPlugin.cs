using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using System.Threading.Tasks;
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
            var view = await CreateCameraReelSectionView();

            controller = new CameraReelSectionController(view, view.GalleryView, view.GalleryStorageView);
            controller.Initialize();
        }

        private static async UniTask<CameraReelSectionView> CreateCameraReelSectionView()
        {
            var assetProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();
            return await assetProvider.Instantiate<CameraReelSectionView>(ADDRESS, ADDRESS,
                parent: DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.Get());
        }

        public void Dispose()
        {
        }
    }
}
