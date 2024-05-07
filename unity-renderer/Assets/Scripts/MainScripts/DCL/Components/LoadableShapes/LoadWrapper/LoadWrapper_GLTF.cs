using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class LoadWrapper_GLTF : LoadWrapper
    {
        private const string SMR_UPDATE_OFFSCREEN_FEATURE_FLAG = "smr_update_offscreen";
        static readonly bool VERBOSE = false;

        RendereableAssetLoadHelper loadHelper;

        public ContentProvider customContentProvider;

        private Action<Rendereable> successWrapperEvent;
        private Action<Exception> failWrapperEvent;

        public override void Load(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper, Exception> OnFail)
        {
            if (loadHelper != null)
            {
                loadHelper.Unload();

                if (VERBOSE)
                    Debug.Log("Forgetting not null loader...");
            }

            alreadyLoaded = false;
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");

            DataStore_FeatureFlag featureFlags = DataStore.i.featureFlags;

            if (customContentProvider == null)
                loadHelper = new RendereableAssetLoadHelper(this.entity.scene.contentProvider, entity.scene.sceneData.baseUrlBundles);
            else
                loadHelper = new RendereableAssetLoadHelper(customContentProvider, entity.scene.sceneData.baseUrlBundles);

            loadHelper.settings.forceGPUOnlyMesh = true;
            loadHelper.settings.parent = entity.meshRootGameObject.transform;
            loadHelper.settings.smrUpdateWhenOffScreen = featureFlags.flags.Get().IsFeatureEnabled(SMR_UPDATE_OFFSCREEN_FEATURE_FLAG);

            if (initialVisibility == false)
            {
                loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
            }
            else
            {
                loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITHOUT_TRANSITION;
            }

            this.entity.OnCleanupEvent -= OnEntityCleanup;
            this.entity.OnCleanupEvent += OnEntityCleanup;

            successWrapperEvent = x => OnSuccessWrapper(OnSuccess);
            failWrapperEvent = error => OnFailWrapper(OnFail, error);

            loadHelper.OnSuccessEvent += successWrapperEvent;
            loadHelper.OnFailEvent += failWrapperEvent;
            loadHelper.Load(targetUrl);
        }

        private void OnFailWrapper(Action<LoadWrapper, Exception> OnFail, Exception exception)
        {
            alreadyLoaded = true;
            loadHelper.OnSuccessEvent -= successWrapperEvent;
            loadHelper.OnFailEvent -= failWrapperEvent;
            this.entity.OnCleanupEvent -= OnEntityCleanup;
            OnFail?.Invoke(this, exception);
        }

        private void OnSuccessWrapper(Action<LoadWrapper> OnSuccess)
        {
            alreadyLoaded = true;
            loadHelper.OnSuccessEvent -= successWrapperEvent;
            loadHelper.OnFailEvent -= failWrapperEvent;

            loadHelper.loadedAsset.ownerId = entity.entityId;
            DataStore.i.sceneWorldObjects.AddRendereable(entity.scene.sceneData.sceneNumber, loadHelper.loadedAsset);
            OnSuccess?.Invoke(this);
        }

        public void OnEntityCleanup(ICleanableEventDispatcher source) { Unload(); }

        public override void Unload()
        {
            if (loadHelper == null)
                return;

            if ( loadHelper.loadedAsset != null )
            {
                DataStore.i.sceneWorldObjects.RemoveRendereable(entity.scene.sceneData.sceneNumber, loadHelper.loadedAsset);
            }

            loadHelper.Unload();
            this.entity.OnCleanupEvent -= OnEntityCleanup;
            loadHelper.OnSuccessEvent -= successWrapperEvent;
            loadHelper.OnFailEvent -= failWrapperEvent;
            alreadyLoaded = false;
        }

        public override string ToString() { return $"LoadWrapper ... {loadHelper.ToString()}"; }
    }
}
