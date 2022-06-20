﻿using System;
using DCL.Components;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class GLTFShapeComponentHandler : IECSComponentHandler<PBGLTFShape>, IShape
    {
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;
        internal PBGLTFShape model;
        internal readonly LoadWrapper_GLTF loadWrapper;
        internal IDCLEntity entity;

        private readonly DataStore_ECS7 dataStore;

        public GLTFShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
        {
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
    
                entity.meshesInfo.currentShape = this;

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
                    dataStore.AddReadyAnimatorShape(entity.entityId,meshesInfo.meshRootGameObject);
                    
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
            // Set visibility
            meshesInfo.meshRootGameObject.SetActive(model.Visible);
            
            // Set collisions
            foreach (var collider in meshesInfo.colliders)
            {
                collider.enabled = model.WithCollisions;
            }
            
            //TODO: Implement events here
        }

        internal void DisposeMesh(IParcelScene scene)
        {
            dataStore.RemoveReadyAnimatorShape(entity.entityId);
            if (meshesInfo != null)
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
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