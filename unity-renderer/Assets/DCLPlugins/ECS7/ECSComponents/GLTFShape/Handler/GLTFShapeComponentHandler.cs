using System;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class GLTFShapeComponentHandler : IECSComponentHandler<PBGLTFShape>
    {
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;
        internal PBGLTFShape model;
        internal readonly LoadWrapper_GLTF loadWrapper;
        internal IDCLEntity entity;

        private readonly ShapeRepresentation shapeRepresentation;
        private readonly DataStore_ECS7 dataStore;

        public GLTFShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
        {
            shapeRepresentation = new ShapeRepresentation();
            dataStore = dataStoreEcs7;
            loadWrapper = new LoadWrapper_GLTF();
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            loadWrapper?.Unload();
            DisposeMesh(scene);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBGLTFShape model)
        {
            this.entity = entity;

            // If we didn't create the shape, or if the shape is different from the last time, we load the shape, if not we update model
            if(ShouldLoadShape(model))
                LoadShape(scene,entity,model);
            else
                ApplyModel(model);
            
            this.model = model;
        }
                
        public bool IsVisible() {  return model.Visible; }
        
        public bool HasCollisions() {  return model.WithCollisions; }

        private bool ShouldLoadShape(PBGLTFShape model)
        {
            // If we didn't create the shape, or if the shape is different from the last time, we load the shape, if not we update model
            return meshesInfo == null || (this.model != null && this.model.Src != model.Src);
        }

        private void LoadShape(IParcelScene scene, IDCLEntity entity, PBGLTFShape model)
        {
            ContentProvider provider = scene.contentProvider;
            if (provider.HasContentsUrl(model.Src))
            {
                entity.EnsureMeshGameObject("GLTF mesh");

                if (loadWrapper != null)
                {
                    loadWrapper.Unload();
                    DisposeMesh(scene);
                }
                
                // We prepare the load wrapper to load the GLTF
                loadWrapper.customContentProvider = provider;
                
                entity.meshesInfo.currentShape = shapeRepresentation;

                loadWrapper.entity = entity;
                loadWrapper.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;
                loadWrapper.initialVisibility = true;
                loadWrapper.Load(model.Src, (wrapper) =>
                {
                    // We remove the transition from the GLTF
                    if (!model.Visible)
                    {
                        MaterialTransitionController[] materialTransitionControllers = entity.meshRootGameObject.GetComponentsInChildren<MaterialTransitionController>();

                        for (var i = 0; i < materialTransitionControllers.Length; i++)
                            GameObject.Destroy(materialTransitionControllers[i]);
                    }
                    entity.meshesInfo.meshRootGameObject = entity.meshRootGameObject;
                    meshesInfo = entity.meshesInfo;
                    
                    // Apply the model for visibility, collision and event pointer
                    ApplyModel(model);
                    dataStore.RemovePendingResource(scene.sceneData.id, model);
                    dataStore.AddShapeReady(entity.entityId,meshesInfo.meshRootGameObject);
                    
                }, (wrapper, exception) =>
                {
                    dataStore.RemovePendingResource(scene.sceneData.id, model);
                });
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"LoadableShape '{model.Src}' not found in scene '{scene.sceneData.id}' mappings");
#endif
            }
            dataStore.AddPendingResource(scene.sceneData.id, model);
        }
        
        internal void ApplyModel(PBGLTFShape model)
        {
            shapeRepresentation.UpdateModel(model.Visible, model.WithCollisions);
            
            // Set visibility
            meshesInfo.meshRootGameObject.SetActive(model.Visible);
            
            // Set collisions and pointer blocker
            ECSComponentsUtils.UpdateMeshInfoColliders(model.WithCollisions, model.IsPointerBlocker, meshesInfo);
        }

        internal void DisposeMesh(IParcelScene scene)
        {
            if (entity != null)
                dataStore.RemoveShapeReady(entity.entityId);
            if (rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id, rendereable);
            if (model != null)
                dataStore.RemovePendingResource(scene.sceneData.id, model);

            meshesInfo = null;
            rendereable = null;
            model = null;
        }
    }
}