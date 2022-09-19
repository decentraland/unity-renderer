﻿using System;
using System.Threading.Tasks;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Helpers.NFT;
using DCL.Models;
using NFTShape_Internal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.ECSComponents
{
    public class ECSNFTShapeComponentHandler : IECSComponentHandler<PBNFTShape>
    {
        internal INFTShapeFrameFactory factory;

        internal INFTInfoRetriever infoRetriever;
        internal INFTAssetRetriever assetRetriever;
        internal INFTShapeFrame shapeFrame;

        private PBNFTShape prevModel;

        private string nftLoadedScr = null;
        private readonly IInternalECSComponent<InternalRenderers> renderersComponent;

        public ECSNFTShapeComponentHandler(INFTShapeFrameFactory factory, INFTInfoRetriever infoRetriever,
            INFTAssetRetriever assetRetriever,
            IInternalECSComponent<InternalRenderers> renderersComponent)
        {
            this.factory = factory;
            this.infoRetriever = infoRetriever;
            this.assetRetriever = assetRetriever;
            this.renderersComponent = renderersComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            infoRetriever.Dispose();
            assetRetriever.Dispose();
            DisposeShapeFrame(scene, entity, shapeFrame, renderersComponent);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBNFTShape model)
        {
            bool shouldReloadFrame = shapeFrame != null && prevModel.GetStyle() != model.GetStyle();

            // destroy previous shape if style does not match
            if (shouldReloadFrame)
            {
                DisposeShapeFrame(scene, entity, shapeFrame, renderersComponent);
                shapeFrame = null;
                nftLoadedScr = null;
            }

            // create shape if it does not exist
            if (shapeFrame == null)
            {
                shapeFrame = factory.InstantiateLoaderController(model.Style);
                shapeFrame.gameObject.name = "NFT Shape mesh";
                shapeFrame.gameObject.transform.SetParent(entity.gameObject.transform);
                shapeFrame.gameObject.transform.ResetLocalTRS();
                renderersComponent.AddRenderer(scene, entity, shapeFrame.frameRenderer);
            }

            if (prevModel == null || !model.Color.Equals(prevModel.Color))
            {
                shapeFrame.UpdateBackgroundColor(new Color(model.Color.R, model.Color.G, model.Color.B, 1));
            }

            if (!string.IsNullOrEmpty(model.Src) && nftLoadedScr != model.Src)
            {
                infoRetriever.Dispose();
                assetRetriever.Dispose();
                LoadNFT(model.Src);
            }

            prevModel = model;
        }

        private static void DisposeShapeFrame(IParcelScene scene, IDCLEntity entity,
            INFTShapeFrame nftFrame, IInternalECSComponent<InternalRenderers> renderersComponent)
        {
            if (nftFrame == null)
                return;

            renderersComponent.RemoveRenderer(scene, entity, nftFrame.frameRenderer);
            nftFrame.Dispose();
            Object.Destroy(nftFrame.gameObject);
        }

        internal async void LoadNFT(string scr)
        {
            try
            {
                NFTInfo info = await infoRetriever.FetchNFTInfo(scr);

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
                nftLoadedScr = scr;
            }
            catch (TaskCanceledException cancelation) { }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private void LoadFailed()
        {
            factory.InstantiateErrorFeedback(shapeFrame.gameObject);
            shapeFrame.FailLoading();
        }
    }
}