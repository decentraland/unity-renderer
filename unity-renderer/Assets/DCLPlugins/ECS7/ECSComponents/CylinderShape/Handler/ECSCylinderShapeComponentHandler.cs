using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSCylinderShapeComponentHandler : IECSComponentHandler<PBCylinderShape>
    {
        internal AssetPromise_PrimitiveMesh primitiveMeshPromisePrimitive;
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;
        internal PBCylinderShape lastModel;
        
        private readonly DataStore_ECS7 dataStore;
        
        public ECSCylinderShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
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

        private PBCylinderShape NormalizeAndCloneModel(PBCylinderShape model)
        {
            PBCylinderShape normalizedModel = model.Clone();

            normalizedModel.Visible = !model.HasVisible || model.Visible;
            normalizedModel.WithCollisions = !model.HasWithCollisions || model.WithCollisions;
            normalizedModel.IsPointerBlocker = !model.HasIsPointerBlocker || model.IsPointerBlocker;
            
            normalizedModel.RadiusTop = model.HasRadiusTop ? model.RadiusTop : 1.0f;
            normalizedModel.RadiusBottom = model.HasRadiusBottom ? model.RadiusBottom : 1.0f;

            return normalizedModel;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBCylinderShape model)
        {
            var normalizedModel = NormalizeAndCloneModel(model);
            if (meshesInfo != null)
            {
                ECSComponentsUtils.UpdateMeshInfo(normalizedModel.Visible, normalizedModel.WithCollisions, normalizedModel.IsPointerBlocker, meshesInfo);
            }
            else
            {
                Mesh generatedMesh = null;
                if (primitiveMeshPromisePrimitive != null)
                    AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);

                PrimitiveMeshModel primitiveMeshModelModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Cylinder);
                primitiveMeshModelModel.radiusBottom = normalizedModel.RadiusBottom;
                primitiveMeshModelModel.radiusTop = normalizedModel.RadiusTop;

                primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
                primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
                {
                    DisposeMesh(entity, scene);
                    generatedMesh = shape.mesh;
                    GenerateRenderer(generatedMesh, scene, entity, normalizedModel.Visible, normalizedModel.WithCollisions, normalizedModel.IsPointerBlocker);
                    dataStore.AddShapeReady(entity.entityId,meshesInfo.meshRootGameObject);
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

            // Note: We should add the rendereable to the data store and dispose when it not longer exists
            rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.id, entity.entityId, mesh, entity.gameObject, meshesInfo.renderers);
        }

        internal void DisposeMesh(IDCLEntity entity,IParcelScene scene)
        {
            if (meshesInfo != null)
            {
                dataStore.RemoveShapeReady(entity.entityId);
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            }
            if (rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id, rendereable);
            if (lastModel != null)
                dataStore.RemovePendingResource(scene.sceneData.id, lastModel);
            
            meshesInfo = null;
            rendereable = null;
        }
    }
}
