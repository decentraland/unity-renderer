using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSBoxShapeComponentHandler : IECSComponentHandler<PBBoxShape>
    {
        internal AssetPromise_PrimitiveMesh primitiveMeshPromisePrimitive;
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;
        internal PBBoxShape lastModel;

        private readonly DataStore_ECS7 dataStore;
        private readonly IInternalECSComponent<InternalTexturizable> texturizableInternalComponent;
        
        public ECSBoxShapeComponentHandler(DataStore_ECS7 dataStoreEcs7, IInternalECSComponent<InternalTexturizable> texturizableInternalComponent)
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
            
            lastModel = null;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBBoxShape model)
        {
            if (lastModel != null && lastModel.Uvs.Equals(model.Uvs))
            {
                ECSComponentsUtils.UpdateMeshInfo(model.GetVisible(), model.GetWithCollisions(), model.GetIsPointerBlocker(), meshesInfo);
            }
            else
            {
                Mesh generatedMesh = null;
                if (primitiveMeshPromisePrimitive != null)
                    AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);

                AssetPromise_PrimitiveMesh_Model primitiveMeshModelModel = AssetPromise_PrimitiveMesh_Model.CreateBox(model.Uvs);
        
                primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
                primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
                {
                    DisposeMesh(entity,scene);
                    generatedMesh = shape.mesh;
                    GenerateRenderer(generatedMesh, scene, entity, model.GetVisible(), model.GetWithCollisions(), model.GetIsPointerBlocker());
                    dataStore.AddShapeReady(entity.entityId, meshesInfo.meshRootGameObject);
                    dataStore.RemovePendingResource(scene.sceneData.id, model);
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

        private void GenerateRenderer(Mesh mesh, IParcelScene scene, IDCLEntity entity, bool isVisible, bool withCollisions, bool isPointerBlocker)
        {
            meshesInfo = ECSComponentsUtils.GeneratePrimitive(entity, mesh, entity.gameObject, isVisible, withCollisions, isPointerBlocker);
            texturizableInternalComponent.AddRenderers(scene, entity, meshesInfo?.renderers);

            // Note: We should add the rendereable to the data store and dispose when it not longer exists
            rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.id, entity.entityId, mesh, entity.gameObject, meshesInfo.renderers);
        }

        internal void DisposeMesh(IDCLEntity entity,IParcelScene scene)
        {
            if (meshesInfo != null)
            {
                texturizableInternalComponent.RemoveRenderers(scene, entity, meshesInfo?.renderers);
                dataStore.RemoveShapeReady(entity.entityId);
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            }
            if(rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id,rendereable);
            if(lastModel != null)
                dataStore.RemovePendingResource(scene.sceneData.id, lastModel);
            
            meshesInfo = null;
            rendereable = null;
        }
    }
}