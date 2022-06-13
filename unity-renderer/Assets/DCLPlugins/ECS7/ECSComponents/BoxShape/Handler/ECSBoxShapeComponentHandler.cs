using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSBoxShapeComponentHandler : IECSComponentHandler<PBBoxShape>
    {
        internal AssetPromise_PrimitiveMesh primitiveMeshPromisePrimitive;
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;
        internal PBBoxShape model;

        internal GameObject meshHolderGameObject;

        private readonly DataStore_ECS7 dataStore;
        
        public ECSBoxShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
        {
            dataStore = dataStoreEcs7;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (primitiveMeshPromisePrimitive != null)
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);
            DisposeMesh(scene);
            if(meshHolderGameObject != null)
                GameObject.Destroy(meshHolderGameObject);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBBoxShape model)
        {
            this.model = model;
            
            Mesh generatedMesh = null;
            if (primitiveMeshPromisePrimitive != null)
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);

            PrimitiveMeshModel primitiveMeshModelModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Box);
            primitiveMeshModelModel.uvs = model.Uvs;

            primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
            primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
            {
                DisposeMesh(scene);
                generatedMesh = shape.mesh;
                GenerateRenderer(generatedMesh, scene, entity, model);
            };
            primitiveMeshPromisePrimitive.OnFailEvent += ( mesh,  exception) =>
            {
                dataStore.RemovePendingResource(scene.sceneData.id, model);
            };
            
            dataStore.AddPendingResource(scene.sceneData.id, model);
            AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromisePrimitive);
        }

        private void GenerateRenderer(Mesh mesh, IParcelScene scene, IDCLEntity entity, PBBoxShape model)
        {
            GenerateHolder(entity);
            meshesInfo = ECSComponentsUtils.GenerateMeshInfo(entity, mesh, meshHolderGameObject, model.Visible, model.WithCollisions, model.IsPointerBlocker);

            // Note: We should add the rendereable to the data store and dispose when it not longer exists
            rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.id, entity.entityId, mesh, entity.gameObject, meshesInfo.renderers);
        }

        private void GenerateHolder(IDCLEntity entity)
        {
            if(meshHolderGameObject != null)
                GameObject.Destroy(meshHolderGameObject);
            
            meshHolderGameObject = new GameObject();
            meshHolderGameObject.transform.SetParent(entity.gameObject.transform,false);
        }

        internal void DisposeMesh(IParcelScene scene)
        {
            if(meshesInfo != null)
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            if(rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id,rendereable);
            if(model != null)
                dataStore.RemovePendingResource(scene.sceneData.id, model);
            
            meshesInfo = null;
            rendereable = null;
        }
    }
}