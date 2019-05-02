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
    public class GLTFShape : BaseLoadableShape<GLTFLoader>
    {
        public GLTFShape(ParcelScene scene) : base(scene)
        {
        }

        protected override void AttachShape(DecentralandEntity entity)
        {
            if (scene.HasContentsUrl(currentSrc))
            {
                entity.EnsureMeshGameObject(componentName + " mesh");
                GLTFLoader loadableShape = entity.meshGameObject.GetOrCreateComponent<GLTFLoader>();
                loadableShape.entity = entity;
                loadableShape.Load(currentSrc, Configuration.ParcelSettings.VISUAL_LOADING_ENABLED);
            }
            else
            {
                Debug.LogError($"GLTF/GLB '{currentSrc}' not found in scene '{scene.sceneData.id}' mappings");
            }
        }
    }
}
