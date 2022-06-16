using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.Helpers;
using DCL.Helpers.NFT;
using NFTShape_Internal;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSNFTShapeComponentHandler : IECSComponentHandler<PBNFTShape>
    {
        internal INFTShapeFrameFactory factory;
        
        internal INFTInfoRetriever infoRetriever;
        internal INFTAssetRetriever assetRetriever;
        internal INFTShapeFrame shapeFrame;

        private PBNFTShape model;
        private DataStore_ECS7 dataStore;
        
        public ECSNFTShapeComponentHandler(INFTShapeFrameFactory factory, DataStore_ECS7 dataStoreEcs7, INFTInfoRetriever infoRetriever, INFTAssetRetriever assetRetriever)
        {
            this.factory = factory;
            dataStore = dataStoreEcs7;

            this.infoRetriever = infoRetriever;
            this.assetRetriever = assetRetriever;
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            infoRetriever.Dispose();
            assetRetriever.Dispose();
            DisposeShapeFrame(entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBNFTShape model)
        {
            // We create the frame gameobject
            if(shapeFrame == null || this.model.Style != model.Style)
                CreateNFTShapeFrame(entity, model);
            
            // We apply the model to the frame
            ApplyModel(model);

            // We load the NFT image
            LoadNFT(scene,model);
        }

        private void DisposeShapeFrame(IDCLEntity entity)
        {
            if (shapeFrame == null)
                return;
            
            shapeFrame.Dispose();
            ECSComponentsUtils.DisposeMeshInfo(entity.meshesInfo);
            GameObject.Destroy(shapeFrame.gameObject);
        }
        
        internal async void LoadNFT(IParcelScene scene,PBNFTShape model)
        {
            dataStore.AddPendingResource(scene.sceneData.id, model);
            
            NFTInfo info = await infoRetriever.FetchNFTInfo(model.Src);

            if (info == null)
            {
                LoadFail(scene, model);
                return;
            }

            INFTAsset nftAsset = await assetRetriever.LoadNFTAsset(info.previewImageUrl);
            
            if (nftAsset == null)
            {
                LoadFail(scene, model);
                return;
            }

            shapeFrame.SetImage(info.name, info.imageUrl, nftAsset);
            
            dataStore.RemovePendingResource(scene.sceneData.id, model);
        }

        private void LoadFail(IParcelScene scene, PBNFTShape model)
        {
            LoadFailed();
            dataStore.RemovePendingResource(scene.sceneData.id, model);
        }
        
        private void CreateNFTShapeFrame(IDCLEntity entity,PBNFTShape model)
        {
            if (shapeFrame != null)
                DisposeShapeFrame(entity);
            
            shapeFrame = factory.InstantiateLoaderController(model.Style);
           
            entity.meshesInfo.meshRootGameObject = shapeFrame.gameObject;
            entity.meshesInfo.currentShape = shapeFrame.shape;
            
            entity.meshesInfo.meshRootGameObject.name = "NFT mesh";
            entity.meshesInfo.meshRootGameObject.transform.SetParent(entity.gameObject.transform);
            entity.meshesInfo.meshRootGameObject.transform.ResetLocalTRS();
        }

        private void LoadFailed()
        {
            factory.InstantiateErrorFeedback(shapeFrame.gameObject);
            shapeFrame.FailLoading();
        }
        
        internal void ApplyModel(PBNFTShape model)
        {
            shapeFrame.SetVisibility(model.Visible);
            shapeFrame.SetHasCollisions(model.WithCollisions);
            UpdateBackgroundColor(model);

            this.model = model;
        }

        internal void UpdateBackgroundColor(PBNFTShape model)
        {
            if (this.model != null && model.Color.Equals(this.model.Color))
                return;

            shapeFrame.UpdateBackgroundColor( new UnityEngine.Color(model.Color.Red, model.Color.Green,model.Color.Blue,1));
        }
    }
}