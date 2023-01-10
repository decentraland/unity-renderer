using System;
using System.Linq;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Components
{
    public class LoadWrapper_NFT : LoadWrapper
    {
        public NFTShapeLoaderController loaderController;

        public bool withCollisions;
        public Color backgroundColor;

        private string assetUrl;
        private Rendereable rendereable;

        public override void Unload()
        {
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            if (loaderController != null)
            {
                loaderController.OnLoadingAssetSuccess -= OnLoadingAssetSuccess;
                Object.Destroy(loaderController);
            }

            if(entity.meshRootGameObject != null)
                Utils.SafeDestroy(entity.meshRootGameObject);
            
            entity.meshesInfo.CleanReferences();

            DataStore.i.sceneWorldObjects.RemoveRendereable(entity.scene.sceneData.sceneNumber, rendereable);
        }

        public override void Load(string src, System.Action<LoadWrapper> OnSuccess, System.Action<LoadWrapper, Exception> OnFail)
        {
            if (string.IsNullOrEmpty(src))
                return;

            if (loaderController == null)
                loaderController = entity.meshRootGameObject.GetComponent<NFTShapeLoaderController>();

            if (loaderController == null)
            {
                Debug.LogError("LoadWrapper_NFT >>> loaderController == null!");
                return;
            }

            loaderController.collider.enabled = withCollisions;
            loaderController.backgroundColor = backgroundColor;

            loaderController.OnLoadingAssetSuccess += OnLoadingAssetSuccess;

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

        void OnLoadingAssetSuccess()
        {
            if (alreadyLoaded)
                return;

            alreadyLoaded = true;

            rendereable = Rendereable.CreateFromGameObject(entity.meshRootGameObject);
            rendereable.ownerId = entity.entityId;
            DataStore.i.sceneWorldObjects.AddRendereable(entity.scene.sceneData.sceneNumber, rendereable);

            entity.OnShapeUpdated?.Invoke(entity);
            entity.OnShapeLoaded?.Invoke(entity);
        }

        void LoadAsset()
        {
            if ( loaderController != null )
                loaderController.LoadAsset(assetUrl, true);
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
