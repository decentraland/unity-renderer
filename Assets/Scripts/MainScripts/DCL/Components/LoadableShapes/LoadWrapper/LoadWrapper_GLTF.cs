using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Components
{
    public class LoadWrapper_GLTF : LoadWrapper
    {
        static bool VERBOSE = false;

        public GameObject gltfContainer;

        string url;
        string assetDirectoryPath;

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

            gltfContainer = AssetManager_GLTF.i.Get(
                id: GetCacheId(),
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
            if (!String.IsNullOrEmpty(url))
            {
                AssetManager_GLTF.i.Release(GetCacheId());
            }
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
