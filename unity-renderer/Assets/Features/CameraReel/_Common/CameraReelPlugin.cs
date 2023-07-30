using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using System;
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
            DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.OnChange += CacheSectionParent;
        }

        private async UniTaskVoid Initialize()
        {
            await CameraReelSectionView.Create(Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>(), sectionParent);
        }

        private void CacheSectionParent(Transform sectionTransform, Transform _)
        {
            sectionParent = sectionTransform;
            DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.OnChange -= CacheSectionParent;
            Debug.Log($"Parent cached in plugin {sectionParent}", sectionParent.gameObject);
        }


        public void Dispose()
        {
            // hudController.Dispose();
        }
    }
}
