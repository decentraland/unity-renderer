using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class LoadWrapper_NFT : LoadWrapper
    {
        [HideInInspector] public NFTShapeLoaderController loaderController;

        public bool withCollisions;
        public Color backgroundColor;
        public BaseDisposable component;

        string assetUrl;

        public override void Unload()
        {
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
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

            assetUrl = src;

            if (CommonScriptableObjects.rendererState.Get())
            {
                LoadAsset();
            }
            else
            {
                CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
            }

            // NOTE: frame meshes are already included in the build, there is no need lock renderer just to wait for the images.
            OnSuccess?.Invoke(this);
        }

        void CallOnComponentUpdated()
        {
            alreadyLoaded = true;

            entity.OnShapeUpdated?.Invoke(entity);
        }

        void LoadAsset()
        {
            loaderController?.LoadAsset(assetUrl, true);
        }

        void OnRendererStateChanged(bool current, bool previous)
        {
            if (current)
            {
                CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
                LoadAsset();
            }
        }
    }
}
