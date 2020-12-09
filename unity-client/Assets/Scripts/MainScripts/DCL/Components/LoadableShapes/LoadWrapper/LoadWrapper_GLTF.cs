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

        public override void Load(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
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

            loadHelper.OnSuccessEvent += (x) => OnSuccessWrapper(OnSuccess);
            loadHelper.OnFailEvent += () => OnFailWrapper(OnSuccess);
            loadHelper.Load(targetUrl);
        }

        private void OnFailWrapper(Action<LoadWrapper> OnFail)
        {
            alreadyLoaded = true;
            OnFail?.Invoke(this);
        }

        private void OnSuccessWrapper(Action<LoadWrapper> OnSuccess)
        {
            alreadyLoaded = true;
            this.entity.OnCleanupEvent -= OnEntityCleanup;
            this.entity.OnCleanupEvent += OnEntityCleanup;
            OnSuccess?.Invoke(this);
        }

        public void OnEntityCleanup(ICleanableEventDispatcher source)
        {
            Unload();
        }

        public override void Unload()
        {
            loadHelper.Unload();
            this.entity.OnCleanupEvent -= OnEntityCleanup;
        }

        public override string ToString()
        {
            return $"LoadWrapper ... {loadHelper.ToString()}";
        }
    }
}
