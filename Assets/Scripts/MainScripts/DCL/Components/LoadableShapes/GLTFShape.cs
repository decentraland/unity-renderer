using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using UnityGLTF;

namespace DCL.Components
{
    public class GLTFLoader : LoadableMonoBehavior
    {
        GLTFComponent gltfLoaderComponent;

        public override void Load(string src, bool useVisualFeedback=false)
        {
            // GLTF Loader can't be reused "out of the box", so we re-instantiate it when needed
            if (gltfLoaderComponent != null)
            {
                Destroy(gltfLoaderComponent);
            }

            alreadyLoaded = false;
            gltfLoaderComponent = gameObject.AddComponent<GLTFComponent>();
            gltfLoaderComponent.OnFinishedLoadingAsset += CallOnComponentUpdatedEvent;
            gltfLoaderComponent.OnFailedLoadingAsset += CallOnFailure;

            gltfLoaderComponent.UseVisualFeedback = useVisualFeedback;
            gltfLoaderComponent.Multithreaded = false;
            gltfLoaderComponent.LoadingTextureMaterial = Utils.EnsureResourcesMaterial("Materials/LoadingTextureMaterial");
            gltfLoaderComponent.LoadAsset(src, true);
        }

        void CallOnFailure()
        {
            gameObject.name += " - Failed loading";
        }

        void CallOnComponentUpdatedEvent()
        {
            alreadyLoaded = true;

            if (entity.OnComponentUpdated != null)
                entity.OnComponentUpdated.Invoke(this);

            if (entity.OnShapeUpdated != null)
                entity.OnShapeUpdated.Invoke(entity);

            BaseShape.ConfigureColliders(entity, true, true);
        }

        void OnDestroy()
        {
            if (gltfLoaderComponent != null)
            {
                gltfLoaderComponent.OnFinishedLoadingAsset -= CallOnComponentUpdatedEvent;
                gltfLoaderComponent.OnFailedLoadingAsset -= CallOnFailure;
            }

            Destroy(gltfLoaderComponent);
        }
    }


    public class GLTFShape : BaseLoadableShape<GLTFLoader>
    {
        public override string componentName => "GLTF Shape";

        public GLTFShape(ParcelScene scene) : base(scene)
        {
        }
    }
}
