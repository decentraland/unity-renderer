using DCL.Helpers;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityGLTF;

namespace DCL.Components
{
    public class LoadWrapper_GLTF : LoadWrapper
    {
        static bool VERBOSE = false;

        public GameObject gltfContainer;

        string url;
        string assetDirectoryPath;

        private static readonly Vector3 MORDOR = Vector3.one * -1000;

        //todo (Alex) Refactor: This flag prevents unload from being called twice. Destroying should be part of the shape detachment
        private bool unloaded = false;

        public override void Load(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");
            if (gltfContainer != null)
            {
                Destroy(gltfContainer);
            }

            alreadyLoaded = false;

            // We separate the directory path of the GLB and its file name, to be able to use the directory path when 
            // fetching relative assets like textures in the ParseGLTFWebRequestedFile() event call
            url = targetUrl.Substring(targetUrl.LastIndexOf('/') + 1);
            assetDirectoryPath = URIHelper.GetDirectoryName(targetUrl);

            if (VERBOSE)
            {
                Debug.Log($"Load(): target URL -> {targetUrl},  url -> {url}, directory path -> {assetDirectoryPath}");
            }

            var config = new UnityGLTF.GLTFComponent.Settings
            {
                OnWebRequestStartEvent = ParseGLTFWebRequestedFile,
                initialVisibility = initialVisibility
            };

            var cacheId = GetCacheId();

            gltfContainer = AssetManager_GLTF.i.Get(
                id: cacheId,
                url: url,
                parent: transform,
                OnSuccess: () => OnSuccessWrapper(this, OnSuccess),
                OnFail: () => OnFailWrapper(this, OnFail),
                config);
        }

        private void OnFailWrapper(LoadWrapper_GLTF gLTFLoader, Action<LoadWrapper> OnFail)
        {
            if (VERBOSE)
            {
                Debug.Log($"Load(): target URL -> {url}. Failure!");
            }

            OnFail?.Invoke(this);
        }

        private void OnSuccessWrapper(LoadWrapper_GLTF gLTFLoader, Action<LoadWrapper> OnSuccess)
        {
            if (VERBOSE)
            {
                Debug.Log($"Load(): target URL -> {url}. Success!");
            }

            alreadyLoaded = true;

            if (entity.meshGameObject)
            {
                PoolableObject po = entity.meshGameObject.GetComponentInChildren<PoolableObject>();

                if (po)
                    this.entity.OnCleanupEvent += po.OnCleanup;
            }

            OnSuccess?.Invoke(this);
        }

        void ParseGLTFWebRequestedFile(ref string requestedFileName)
        {
            string finalURL = string.Empty;
            contentProvider.TryGetContentsUrl(assetDirectoryPath + requestedFileName, out finalURL);
            requestedFileName = finalURL;
        }


        public object GetCacheId()
        {
            return AssetManager_GLTF.i.GetIdForAsset(contentProvider, url);
        }

        public override void Unload()
        {
            if (unloaded) return;

            unloaded = true;

            if (!String.IsNullOrEmpty(url))
            {
                var cacheId = GetCacheId();

                if (AssetManager_GLTF.i.assetLibrary.ContainsKey(cacheId) && !AssetManager_GLTF.i.assetLibrary[cacheId].isLoadingCompleted)
                {
                    AssetManager_GLTF.i.assetLibrary[cacheId].OnSuccess += OnSuccessAssetLoaded;
                    RelocateLoader();
                }
                else
                {
                    RemoveMeshObject();
                }

                AssetManager_GLTF.i.Release(cacheId);
            }
        }

        private void OnSuccessAssetLoaded()
        {
            var cacheId = GetCacheId();

            AssetManager_GLTF.i.assetLibrary[cacheId].OnSuccess -= OnSuccessAssetLoaded;

            RemoveMeshObject();
        }

        private void RelocateLoader()
        {
            var gltfComponent = GetComponentInChildren<GLTFComponent>();

            if (gltfComponent != null && AssetManager_GLTF.i != null)
            {
                gltfComponent.transform.position = MORDOR;
            }
        }

        private void RemoveMeshObject()
        {
            entity.Cleanup();
        }

        public void OnDestroy()
        {
            if (Application.isPlaying)
            {
                Unload();
            }
        }
    }
}
