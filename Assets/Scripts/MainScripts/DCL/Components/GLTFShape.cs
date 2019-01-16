using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityGLTF;

namespace DCL.Components
{
    public class GLTFShape : BaseShape
    {
        [System.Serializable]
        public class Model
        {
            public string src;
        }

        public bool alreadyLoaded { get; private set; }

        Model model = new Model();
        GLTFComponent gltfLoaderComponent;

        protected new void Awake()
        {
            base.Awake();

            if (meshFilter)
            {
#if UNITY_EDITOR
                DestroyImmediate(meshFilter);
#else
        Destroy(meshFilter);
#endif
            }

            if (meshRenderer)
            {
#if UNITY_EDITOR
                DestroyImmediate(meshRenderer);
#else
        Destroy(meshRenderer);
#endif
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Helpers.Utils.SafeFromJson<Model>(newJson); // We don't use FromJsonOverwrite() to default the model properties on a partial json.

            if (!string.IsNullOrEmpty(model.src))
            {
                // GLTF Loader can't be reused "out of the box", so we re-instantiate it when needed
                if (gltfLoaderComponent != null)
                {
                    Destroy(gltfLoaderComponent);
                }

                alreadyLoaded = false;
                gltfLoaderComponent = meshGameObject.AddComponent<GLTFComponent>();
                gltfLoaderComponent.OnFinishedLoadingAsset += OnFinishedLoadingAsset;

                gltfLoaderComponent.Multithreaded = false;
                gltfLoaderComponent.LoadAsset(model.src, true);

                if (gltfLoaderComponent.loadingPlaceholder == null)
                {
                    gltfLoaderComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(gameObject.transform);
                }
                else
                {
                    gltfLoaderComponent.loadingPlaceholder.SetActive(true);
                }
            }

            return null;
        }

        public override IEnumerator UpdateComponent(string newJson)
        {
            yield return ApplyChanges(newJson);
        }

        void OnFinishedLoadingAsset()
        {
            ConfigureCollision(true, true);

            alreadyLoaded = true;

            if (entity.OnComponentUpdated != null)
                entity.OnComponentUpdated.Invoke(this);
        }

        protected override void OnDestroy()
        {
            if (gltfLoaderComponent != null)
            {
                gltfLoaderComponent.OnFinishedLoadingAsset -= OnFinishedLoadingAsset;
            }

            base.OnDestroy();

            Destroy(gltfLoaderComponent);
        }
    }
}
