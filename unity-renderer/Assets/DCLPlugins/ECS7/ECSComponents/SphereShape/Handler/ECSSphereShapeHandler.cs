﻿using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSSphereShapeComponentHandler : IECSComponentHandler<PBSphereShape>
    {
        internal AssetPromise_PrimitiveMesh primitiveMeshPromisePrimitive;
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;
        internal PBSphereShape lastModel;
        
        private readonly DataStore_ECS7 dataStore;
        private readonly IInternalECSComponent<InternalTexturizable> texturizableInternalComponent;
        
        public ECSSphereShapeComponentHandler(DataStore_ECS7 dataStoreEcs7, IInternalECSComponent<InternalTexturizable> texturizableInternalComponent)
        {
            dataStore = dataStoreEcs7;
            this.texturizableInternalComponent = texturizableInternalComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (primitiveMeshPromisePrimitive != null)
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);
            DisposeMesh(entity, scene);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBSphereShape model)
        {
            if (meshesInfo != null)
            {
                ECSComponentsUtils.UpdateMeshInfo(model.GetVisible(), model.GetWithCollisions(), model.GetIsPointerBlocker(), meshesInfo);
            }
            else
            {
                Mesh generatedMesh = null;
                if (primitiveMeshPromisePrimitive != null)
                    AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);

                AssetPromise_PrimitiveMesh_Model primitiveMeshModelModel = AssetPromise_PrimitiveMesh_Model.CreateSphere();
                primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
                primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
                {
                    DisposeMesh(entity, scene);
                    generatedMesh = shape.mesh;
                    GenerateRenderer(generatedMesh, scene, entity, model);
                    dataStore.RemovePendingResource(scene.sceneData.sceneNumber, model);
                    dataStore.AddShapeReady(entity.entityId,meshesInfo.meshRootGameObject);
                };
                primitiveMeshPromisePrimitive.OnFailEvent += ( mesh,  exception) =>
                {
                    dataStore.RemovePendingResource(scene.sceneData.sceneNumber, model);
                };
            
                dataStore.AddPendingResource(scene.sceneData.sceneNumber, model);
                
                AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromisePrimitive);
            }
            lastModel = model;
        }

        private void GenerateRenderer(Mesh mesh, IParcelScene scene, IDCLEntity entity, PBSphereShape model)
        {
            meshesInfo = ECSComponentsUtils.GeneratePrimitive(entity, mesh, entity.gameObject, model.GetVisible(), model.GetWithCollisions(), model.GetIsPointerBlocker());
            texturizableInternalComponent.AddRenderers(scene, entity, meshesInfo?.renderers);
            
            // Note: We should add the rendereable to the data store and dispose when it not longer exists
            rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.sceneNumber, entity.entityId, mesh, entity.gameObject, meshesInfo.renderers);
        }
        
        internal void DisposeMesh(IDCLEntity entity, IParcelScene scene)
        {
            if (meshesInfo != null)
            {
                texturizableInternalComponent.RemoveRenderers(scene, entity, meshesInfo?.renderers);
                dataStore.RemoveShapeReady(entity.entityId);
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            }
            if(rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.sceneNumber,rendereable);
            if (lastModel != null)
                dataStore.RemovePendingResource(scene.sceneData.sceneNumber, lastModel);
            
            meshesInfo = null;
            rendereable = null;
        }
    }
}