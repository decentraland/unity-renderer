using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityGLTF;

namespace DCL.Components
{
    public class GLTFLoader : LoadableMonoBehavior
    {
        static bool VERBOSE = false;

        public GameObject gltfContainer;

        string url;
        string assetDirectoryPath;
        public Action<GLTFComponent> OnGLTFConfig;

        public override void Load(string targetUrl, Action<LoadableMonoBehavior> OnSuccess, Action<LoadableMonoBehavior> OnFail)
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

            gltfContainer = AssetManager_GLTF.i.Get(
                id: GetCacheId(),
                url: url,
                parent: transform,
                OnSuccess: () => OnSuccessWrapper(this, OnSuccess),
                OnFail: () => OnFailWrapper(this, OnFail),
                webRequestStartEventAction: ParseGLTFWebRequestedFile,
                initialVisibility: initialVisibility);

            AssetManager_GLTF.i.OnStartLoading -= OnGLTFConfig_Internal;
            AssetManager_GLTF.i.OnStartLoading += OnGLTFConfig_Internal;
        }

        void OnGLTFConfig_Internal(GLTFComponent gltfComponent)
        {
            if (gltfComponent.GLTFUri == url)
            {
                OnGLTFConfig?.Invoke(gltfComponent);
                AssetManager_GLTF.i.OnStartLoading -= OnGLTFConfig_Internal;
            }
        }

        private void OnFailWrapper(GLTFLoader gLTFLoader, Action<LoadableMonoBehavior> OnFail)
        {
            if (VERBOSE)
            {
                Debug.Log($"Load(): target URL -> {url}. Failure!");
            }

            AssetManager_GLTF.i.OnStartLoading -= OnGLTFConfig_Internal;
            OnFail?.Invoke(this);
        }

        private void OnSuccessWrapper(GLTFLoader gLTFLoader, Action<LoadableMonoBehavior> OnSuccess)
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

            AssetManager_GLTF.i.OnStartLoading -= OnGLTFConfig_Internal;
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
