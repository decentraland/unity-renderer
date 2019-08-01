using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class LoadWrapper_NFT : LoadWrapper
    {
        [HideInInspector] public NFTShapeLoaderController loaderController;

        void Awake()
        {
            loaderController = GetComponent<NFTShapeLoaderController>();
            loaderController.OnLoadingAssetSuccess += CallOnComponentUpdated;
        }

        public override void Unload()
        {
            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;
        }

        public override void Load(string src, System.Action<LoadWrapper> OnSuccess, System.Action<LoadWrapper> OnFail)
        {
            if (string.IsNullOrEmpty(src)) return;

            loaderController.OnLoadingAssetSuccess += () => OnSuccess(this);
            loaderController.OnLoadingAssetFail += () => OnFail(this);

            loaderController.LoadAsset(src, true);
        }

        void CallOnComponentUpdated()
        {
            alreadyLoaded = true;

            entity.OnComponentUpdated?.Invoke(this);
            entity.OnShapeUpdated?.Invoke(entity);
        }

        void OnDestroy()
        {
            loaderController.OnLoadingAssetSuccess -= CallOnComponentUpdated;

            Destroy(loaderController);
        }
    }
}
