#define USE_FILENAME_AS_HASH

using System;
using UnityEngine;

namespace DCL.Components
{
    public class GLTFLoader : LoadableMonoBehavior
    {
        GameObject gltfContainer;
        string url;
        string assetDirectoryPath;

        bool VERBOSE = false;

        public override void Load(string targetUrl, bool useVisualFeedback = false)
        {
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

            gltfContainer = AssetManager_GLTF.i.Get(GetCacheId(), url, transform, CallOnComponentUpdatedEvent, CallOnFailure, ParseGLTFWebRequestedFile);
        }

        void ParseGLTFWebRequestedFile(ref string requestedFileName)
        {
            string finalURL = string.Empty;
            entity.scene.TryGetContentsUrl(assetDirectoryPath + requestedFileName, out finalURL);
            requestedFileName = finalURL;
        }

        public object GetCacheId()
        {
            return AssetManager_GLTF.i.GetIdForAsset(entity.scene.sceneData, url);
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

        void CallOnFailure()
        {
            gameObject.name += " - Failed loading";

            MaterialTransitionController[] transitionController = GetComponentsInChildren<MaterialTransitionController>(true);

            foreach (MaterialTransitionController material in transitionController)
            {
                Destroy(material);
            }
        }

        void CallOnComponentUpdatedEvent()
        {
            alreadyLoaded = true;
            BaseShape.ConfigureVisibility(entity.meshGameObject, ((GLTFShape)entity.currentShape).model.visible);

            if (entity.OnComponentUpdated != null)
            {
                entity.OnComponentUpdated.Invoke(this);
            }

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }
        }
    }
}
