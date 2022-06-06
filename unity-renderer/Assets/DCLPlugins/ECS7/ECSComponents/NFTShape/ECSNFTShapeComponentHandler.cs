using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSNFTShapeComponentHandler : IECSComponentHandler<PBNFTShape>
    {
        private INFTInfoLoadHelper infoLoadHelper;
        private INFTAssetLoadHelper assetLoadHelper;
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;

        private readonly DataStore_ECS7 dataStore;
        
        public ECSNFTShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
        {
            dataStore = dataStoreEcs7;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
           
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBNFTShape model)
        {

        }
        
        private void GenerateRenderer(Mesh mesh, IParcelScene scene, IDCLEntity entity, PBNFTShape model)
        {
            meshesInfo = ECSComponentsUtils.GenerateMeshInfo(entity, mesh, entity.gameObject, model.Visible, model.WithCollisions, model.IsPointerBlocker);

            // Note: We should add the rendereable to the data store and dispose when it not longer exists
            rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.id, entity.entityId, mesh, entity.gameObject, meshesInfo.renderers);
        }
    }
}