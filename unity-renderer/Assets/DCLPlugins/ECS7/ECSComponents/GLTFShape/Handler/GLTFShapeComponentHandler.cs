using System;
using System.Linq;
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
        internal LoadWrapper_GLTF loadWrapper;
        internal IDCLEntity entity;

        private readonly ShapeRepresentation shapeRepresentation;
        private readonly DataStore_ECS7 dataStore;
        private readonly CollidersManager collidersManager;

        public GLTFShapeComponentHandler(DataStore_ECS7 dataStoreEcs7, CollidersManager collidersManager)
        {
            this.collidersManager = collidersManager;
            shapeRepresentation = new ShapeRepresentation();
            dataStore = dataStoreEcs7;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Dispose(scene);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBGLTFShape model)
        {
            if (this.model != null && this.model.Equals(model))
                return;
            
            this.entity = entity;
            
            // If we didn't create the shape, or if the shape is different from the last time, we load the shape, if not we update model
            if(ShouldLoadShape(model))
                LoadShape(scene,entity,model);
            else
                ApplyModel(entity, model);
            
            this.model = model;
        }

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
                    Dispose(scene);
                
                // We prepare the load wrapper to load the GLTF
                loadWrapper = new LoadWrapper_GLTF
                {
                    customContentProvider = provider,
                    entity = entity,
                    useVisualFeedback = ParcelSettings.VISUAL_LOADING_ENABLED,
                    initialVisibility = true
                };

                // We assign the shape representation to the entity 
                entity.meshesInfo.currentShape = shapeRepresentation;
                
                // Start loading the model
                loadWrapper.Load(model.Src, (wrapper) =>
                {
                    // We remove the transition from the GLTF if it is not visible
                    if (!model.GetVisible())
                        ECSComponentsUtils.RemoveMaterialTransition(entity.meshRootGameObject);
                    
                    entity.meshesInfo.meshRootGameObject = entity.meshRootGameObject;
                    meshesInfo = entity.meshesInfo;
                    
                    // We get the colliders of the GLTF, if it doesn't have colliders and it needed it, we will create in the ApplyModel
                    collidersManager.CreateColliders(entity.meshRootGameObject, meshesInfo.meshFilters, model.GetWithCollisions(), model.GetIsPointerBlocker(), entity, ECSComponentsUtils.CalculateCollidersLayer(model.GetWithCollisions(),model.GetIsPointerBlocker()));
                    
                    // Apply the model for visibility, collision and event pointer
                    ApplyModel(entity, model);
                    
                    // We remove the asset loading and notify of the shape ready
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
        
        internal void ApplyModel(IDCLEntity entity, PBGLTFShape model)
        {
            shapeRepresentation.UpdateModel(model.GetVisible(), model.GetWithCollisions());
            
            // If the model didn't had collider because the first model came with WithCollisions = false and IsPointerBlocker = false
            if(meshesInfo.colliders.Count == 0 && meshesInfo.meshFilters.Length > 0)
                collidersManager.CreateColliders(entity.meshRootGameObject, meshesInfo.meshFilters, model.GetWithCollisions(), model.GetIsPointerBlocker(), entity, ECSComponentsUtils.CalculateCollidersLayer(model.GetWithCollisions(), model.GetIsPointerBlocker()));
            
            // Update the mesh info 
            ECSComponentsUtils.UpdateMeshInfo(entity, model.GetVisible(), model.GetWithCollisions(), model.GetIsPointerBlocker(), meshesInfo);
        }

        internal void Dispose(IParcelScene scene)
        {
            if (meshesInfo != null)
            {
                // We dispose the colliders
                ECSComponentsUtils.DisposeColliders(meshesInfo.colliders);
                
                // Clean the references to the entity mesh info
                meshesInfo.CleanReferences();
            }

            // We notify that the shape is not ready anymore, so others component can be notify
            if (entity != null)
                dataStore.RemoveShapeReady(entity.entityId);
            
            // If we are loading and we dispose, we notify that we don't have a pending resource
            if (model != null)
                dataStore.RemovePendingResource(scene.sceneData.id, model);

            meshesInfo = null;
            rendereable = null;
            model = null;
            
            // We unload the loader wrapper so it can handle the GLTF asset management
            if(loadWrapper != null)
                loadWrapper.Unload();
        }
    }
}