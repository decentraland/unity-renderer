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
        
        internal GameObject meshHolderGameObject;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (primitiveMeshPromisePrimitive != null)
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);
            DisposeMesh(scene);
            
            if(meshHolderGameObject != null)
                GameObject.Destroy(meshHolderGameObject);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBCylinderShape model)
        {
            Mesh generatedMesh = null;
            if (primitiveMeshPromisePrimitive != null)
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);

            PrimitiveMeshModel primitiveMeshModelModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Cylinder);
            primitiveMeshModelModel.radiusBottom = model.RadiusBottom;
            primitiveMeshModelModel.radiusTop = model.RadiusTop;
            
            primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
            primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
            {
                DisposeMesh(scene);
                generatedMesh = shape.mesh;
                GenerateRenderer(generatedMesh, scene, entity, model);
            };
            AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromisePrimitive);
        }

        private void GenerateRenderer(Mesh mesh, IParcelScene scene, IDCLEntity entity, PBCylinderShape model)
        {
            GenerateHolder(entity);
            meshesInfo = ECSComponentsUtils.GenerateMeshInfo(entity, mesh, entity.gameObject, model.Visible, model.WithCollisions, model.IsPointerBlocker);

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
            if (meshesInfo != null)
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            if (rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id, rendereable);

            meshesInfo = null;
            rendereable = null;
        }
    }
}
