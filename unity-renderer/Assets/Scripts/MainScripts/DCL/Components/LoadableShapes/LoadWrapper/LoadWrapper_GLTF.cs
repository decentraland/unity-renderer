using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class LoadWrapper_GLTF : LoadWrapper
    {
        static readonly bool VERBOSE = false;

        RendereableAssetLoadHelper loadHelper;

        public ContentProvider customContentProvider;

        private Action<GameObject> successWrapperEvent;
        private Action failWrapperEvent;

        /// <summary>
        /// Tracked meshes to persist in DataStore_SceneRendering
        /// </summary>
        public HashSet<Mesh> trackedMeshes = new HashSet<Mesh>();

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

            successWrapperEvent = (x) => OnSuccessWrapper(OnSuccess);
            failWrapperEvent = () => OnFailWrapper(OnFail);

            loadHelper.OnSuccessEvent += successWrapperEvent;
            loadHelper.OnFailEvent += failWrapperEvent;
            loadHelper.OnMeshAdded += OnMeshAdded;
            loadHelper.Load(targetUrl);
        }

        private void OnMeshAdded(Mesh mesh)
        {
            if ( trackedMeshes.Contains(mesh) )
                return;

            trackedMeshes.Add(mesh);
            DataStore.i.sceneRendering.AddMesh(entity, mesh);
        }

        private void OnFailWrapper(Action<LoadWrapper> OnFail)
        {
            alreadyLoaded = true;
            loadHelper.OnMeshAdded -= OnMeshAdded;
            loadHelper.OnSuccessEvent -= successWrapperEvent;
            loadHelper.OnFailEvent -= failWrapperEvent;
            this.entity.OnCleanupEvent -= OnEntityCleanup;
            OnFail?.Invoke(this);
        }

        private void OnSuccessWrapper(Action<LoadWrapper> OnSuccess)
        {
            alreadyLoaded = true;
            loadHelper.OnMeshAdded -= OnMeshAdded;
            loadHelper.OnSuccessEvent -= successWrapperEvent;
            loadHelper.OnFailEvent -= failWrapperEvent;
            OnSuccess?.Invoke(this);
        }

        public void OnEntityCleanup(ICleanableEventDispatcher source) { Unload(); }

        public override void Unload()
        {
            foreach ( var mesh in trackedMeshes )
            {
                DataStore.i.sceneRendering.RemoveMesh(entity, mesh);
            }

            trackedMeshes.Clear();
            loadHelper.Unload();
            this.entity.OnCleanupEvent -= OnEntityCleanup;
            loadHelper.OnMeshAdded -= OnMeshAdded;
            loadHelper.OnSuccessEvent -= successWrapperEvent;
            loadHelper.OnFailEvent -= failWrapperEvent;
            alreadyLoaded = false;
        }

        public override string ToString() { return $"LoadWrapper ... {loadHelper.ToString()}"; }
    }
}