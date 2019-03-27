#define USE_FILENAME_AS_HASH

using System;
using UnityEngine;

namespace DCL.Components
{
    public class GLTFLoader : LoadableMonoBehavior
    {
        GameObject gltfContainer;
        string url;

        public override void Load(string url, bool useVisualFeedback = false)
        {
            if (gltfContainer != null)
            {
                Destroy(gltfContainer);
            }

            alreadyLoaded = false;
            this.url = url;
            gltfContainer = AssetManager_GLTF.i.Get(GetCacheId(), url, transform, CallOnComponentUpdatedEvent, CallOnFailure);
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

            MaterialTransitionController[] c = GetComponentsInChildren<MaterialTransitionController>(true);

            foreach (MaterialTransitionController m in c)
            {
                Destroy(m);
            }
        }

        void CallOnComponentUpdatedEvent()
        {
            alreadyLoaded = true;

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
