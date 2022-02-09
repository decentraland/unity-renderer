using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class LoadWrapper_GLTF : LoadWrapper
    {
        static readonly bool VERBOSE = false;

        RendereableAssetLoadHelper loadHelper;

        public ContentProvider customContentProvider;

        private Action<Rendereable> successWrapperEvent;
        private Action<Exception> failWrapperEvent;

        private const string AB_FEATURE_FLAG_NAME = "asset_bundles";
        private bool useAssetBundles = true;

        public LoadWrapper_GLTF()
        {
            useAssetBundles = UseAssetBundles();
        }

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


            if (customContentProvider == null)
                loadHelper = new RendereableAssetLoadHelper(this.entity.scene.contentProvider, entity.scene.sceneData.baseUrlBundles);
            else
                loadHelper = new RendereableAssetLoadHelper(customContentProvider, entity.scene.sceneData.baseUrlBundles);

            loadHelper.settings.forceGPUOnlyMesh = true;
            loadHelper.settings.parent = entity.meshRootGameObject.transform;

            if (initialVisibility == false)
            {
                loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
            }
            else
            {
                if (useVisualFeedback)
                    loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION;
                else
                    loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITHOUT_TRANSITION;
            }

            this.entity.OnCleanupEvent -= OnEntityCleanup;
            this.entity.OnCleanupEvent += OnEntityCleanup;

            successWrapperEvent = x => OnSuccessWrapper(OnSuccess);
            failWrapperEvent = error => OnFailWrapper(OnFail, error); 

            loadHelper.OnSuccessEvent += successWrapperEvent;
            loadHelper.OnFailEvent += failWrapperEvent;
            loadHelper.Load(targetUrl, useAssetBundles);
        }

        private bool UseAssetBundles()
        {
            var featureFlags = DataStore.i.featureFlags.flags.Get();
            return featureFlags != null && featureFlags.IsFeatureEnabled(AB_FEATURE_FLAG_NAME);
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

            DataStore.i.sceneWorldObjects.AddRendereable(entity, loadHelper.loadedAsset);
            OnSuccess?.Invoke(this);
        }

        public void OnEntityCleanup(ICleanableEventDispatcher source) { Unload(); }

        public override void Unload()
        {
            if ( loadHelper.loadedAsset != null )
            {
                DataStore.i.sceneWorldObjects.RemoveRendereable(entity, loadHelper.loadedAsset);
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