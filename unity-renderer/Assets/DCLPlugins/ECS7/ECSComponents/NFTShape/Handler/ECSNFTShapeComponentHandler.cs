using System.Collections.Generic;
using DCL.Configuration;
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

        public DataStore_ECS7 dataStore;
        
        public ECSNFTShapeComponentHandler(DataStore_ECS7 dataStore, INFTShapeFrameFactory factory, INFTInfoRetriever infoRetriever, INFTAssetRetriever assetRetriever)
        {
            this.dataStore = dataStore;
            this.factory = factory;
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
            if (model.Equals(this.model))
                return;
            
            // We create the frame GameObject
            if(shapeFrame == null || this.model?.Style != model.Style)
                CreateNFTShapeFrame(entity, model);
            
            // We apply the model to the frame
            ApplyModel(model);

            // We load the NFT image
            LoadNFT(model);
        }

        private void DisposeShapeFrame(IDCLEntity entity)
        {
            if (shapeFrame == null)
                return;
            
            dataStore.RemoveShapeReady(entity.entityId);
            shapeFrame.Dispose();
            ECSComponentsUtils.DisposeMeshInfo(entity.meshesInfo);
            GameObject.Destroy(shapeFrame.gameObject);
        }
        
        internal async void LoadNFT(PBNFTShape model)
        {
            NFTInfo info = await infoRetriever.FetchNFTInfo(model.Src);

            if (info == null)
            {
                LoadFailed();
                return;
            }

            INFTAsset nftAsset = await assetRetriever.LoadNFTAsset(info.previewImageUrl);
            
            if (nftAsset == null)
            {
                LoadFailed();
                return;
            }

            shapeFrame.SetImage(info.name, info.imageUrl, nftAsset);
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
            entity.meshesInfo.renderers = new Renderer[] { shapeFrame.GetRenderer() };
            CollidersManager.i.AddOrUpdateEntityCollider(entity, shapeFrame.nftCollider);
            dataStore.AddShapeReady(entity.entityId, entity.meshesInfo.meshRootGameObject);
        }

        private void LoadFailed()
        {
            if (shapeFrame == null)
                return;
            
            factory.InstantiateErrorFeedback(shapeFrame.gameObject);
            shapeFrame.FailLoading();
        }
        
        internal void ApplyModel(PBNFTShape model)
        {
            // If we have collisions or we need to block the pointer event, we activate the collider
            bool shouldHaveCollider = model.GetWithCollisions() || model.GetIsPointerBlocker();
            shapeFrame.UpdateCollider(shouldHaveCollider);
            
            shapeFrame.SetVisibility(model.GetVisible());
            
            int layer = ECSComponentsUtils.CalculateCollidersLayer(model.GetWithCollisions(), model.GetIsPointerBlocker());
            shapeFrame.SetLayer(layer);
            UpdateBackgroundColor(model);

            this.model = model;
        }

        internal void UpdateBackgroundColor(PBNFTShape model)
        {
            if (this.model != null && model.Color.Equals(this.model.Color))
                return;

            shapeFrame.UpdateBackgroundColor( new UnityEngine.Color(model.Color.R, model.Color.G,model.Color.B,1));
        }
    }
}