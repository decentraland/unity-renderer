using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class GLTFShape : BaseLoadableShape<GLTFLoader>
    {
        public override string componentName => "GLTF Shape";

        public GLTFShape(ParcelScene scene) : base(scene)
        {
        }

        protected override void AttachShape(DecentralandEntity entity)
        {
            if (scene.contentProvider.HasContentsUrl(currentSrc))
            {
                entity.EnsureMeshGameObject(componentName + " mesh");
                GLTFLoader loadableShape = entity.meshGameObject.GetOrCreateComponent<GLTFLoader>();
                loadableShape.entity = entity;
                loadableShape.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;
                loadableShape.initialVisibility = model.visible;
                loadableShape.contentProvider = scene.contentProvider;
                loadableShape.Load(currentSrc, OnLoadCompleted, OnLoadFailed);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"GLTF/GLB '{currentSrc}' not found in scene '{scene.sceneData.id}' mappings");
#endif
            }
        }
    }
}
