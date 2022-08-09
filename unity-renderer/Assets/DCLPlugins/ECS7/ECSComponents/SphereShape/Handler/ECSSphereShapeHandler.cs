﻿using DCL.Controllers;
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
        
        public ECSSphereShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
        {
            dataStore = dataStoreEcs7;
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

                PrimitiveMeshModel primitiveMeshModelModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Sphere);
                primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
                primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
                {
                    DisposeMesh(entity, scene);
                    generatedMesh = shape.mesh;
                    GenerateRenderer(generatedMesh, scene, entity, model);
                    dataStore.RemovePendingResource(scene.sceneData.id, model);
                    dataStore.AddShapeReady(entity.entityId,meshesInfo.meshRootGameObject);
                };
                primitiveMeshPromisePrimitive.OnFailEvent += ( mesh,  exception) =>
                {
                    dataStore.RemovePendingResource(scene.sceneData.id, model);
                };
            
                dataStore.AddPendingResource(scene.sceneData.id, model);
                
                AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromisePrimitive);
            }
            lastModel = model;
        }

        private void GenerateRenderer(Mesh mesh, IParcelScene scene, IDCLEntity entity, PBSphereShape model)
        {
            meshesInfo = ECSComponentsUtils.GeneratePrimitive(entity, mesh, entity.gameObject, model.GetVisible(), model.GetWithCollisions(), model.GetIsPointerBlocker());

            // Note: We should add the rendereable to the data store and dispose when it not longer exists
            rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.id, entity.entityId, mesh, entity.gameObject, meshesInfo.renderers);
        }
        
        internal void DisposeMesh(IDCLEntity entity, IParcelScene scene)
        {
            if (meshesInfo != null)
            {
                dataStore.RemoveShapeReady(entity.entityId);
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            }
            if(rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id,rendereable);
            if (lastModel != null)
                dataStore.RemovePendingResource(scene.sceneData.id, lastModel);
            
            meshesInfo = null;
            rendereable = null;
        }
    }
}