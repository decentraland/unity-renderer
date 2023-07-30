using Cysharp.Threading.Tasks;
using DCL.Providers;
using UnityEngine;
using Environment = DCL.Environment;

namespace CameraReel
{
    public class CameraReelPlugin: IPlugin
    {
        private Transform sectionParent;

        public CameraReelPlugin()
        {
            Initialize().Forget();
        }

        private static async UniTaskVoid Initialize()
        {
            await CameraReelSectionView.Create(Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>());
        }

        public void Dispose()
        {
        }
    }
}
