using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class LoadWrapper_NFT : LoadWrapper
    {
        [HideInInspector] public NFTShapeLoaderController loaderController;

        public bool withCollisions;
        public Color backgroundColor;

        public override void Unload()
        {
            loaderController.OnLoadingAssetSuccess -= CallOnComponentUpdated;
            Object.Destroy(loaderController);

            Utils.SafeDestroy(entity.meshRootGameObject);
            entity.meshesInfo.CleanReferences();
        }

        public override void Load(string src, System.Action<LoadWrapper> OnSuccess, System.Action<LoadWrapper> OnFail)
        {
            if (string.IsNullOrEmpty(src)) return;

            if (loaderController == null)
                loaderController = entity.meshRootGameObject.GetComponent<NFTShapeLoaderController>();

            if (loaderController == null)
            {
                Debug.LogError("LoadWrapper_NFT >>> loaderController == null!");
                return;
            }

            loaderController.collider.enabled = withCollisions;
            loaderController.backgroundColor = backgroundColor;

            loaderController.OnLoadingAssetSuccess += CallOnComponentUpdated;

            loaderController.OnLoadingAssetSuccess += () => OnSuccess(this);
            loaderController.OnLoadingAssetFail += () => OnFail(this);

            loaderController.LoadAsset(src, true);
        }

        void CallOnComponentUpdated()
        {
            alreadyLoaded = true;

            entity.OnShapeUpdated?.Invoke(entity);
        }
    }
}
