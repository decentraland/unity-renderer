﻿using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Helpers.NFT;
using DCL.Models;
using Decentraland.Common;
using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;
using NFTShape_Internal;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.ECSComponents
{
    public class ECSNFTShapeComponentHandler : IECSComponentHandler<PBNftShape>
    {
        internal INFTShapeFrameFactory factory;

        internal INFTInfoRetriever infoRetriever;
        internal INFTAssetRetriever assetRetriever;
        internal INFTShapeFrame shapeFrame;

        private PBNftShape prevModel;

        private string nftLoadedScr = null;
        private bool infoRetrieverDisposed = false;
        private bool assetRetrieverDisposed = false;
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
            if (!infoRetrieverDisposed)
            {
                infoRetriever.Dispose();
            }
            if (!assetRetrieverDisposed)
            {
                assetRetriever.Dispose();
            }
            DisposeShapeFrame(scene, entity, shapeFrame, renderersComponent);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBNftShape model)
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
                int style = (int)model.GetStyle();
                shapeFrame = factory.InstantiateLoaderController(style);
                shapeFrame.gameObject.name = "NFT Shape mesh";
                shapeFrame.gameObject.transform.SetParent(entity.gameObject.transform);
                shapeFrame.gameObject.transform.ResetLocalTRS();
                renderersComponent.AddRenderer(scene, entity, shapeFrame.frameRenderer);
            }

            Color3 modelColor = model.GetColor();
            if (prevModel == null || !modelColor.Equals(prevModel.GetColor()))
            {
                shapeFrame.UpdateBackgroundColor(new Color(modelColor.R, modelColor.G, modelColor.B, 1));
            }

            if (!string.IsNullOrEmpty(model.Urn) && nftLoadedScr != model.Urn)
            {
                if (!infoRetrieverDisposed)
                {
                    infoRetriever.Dispose();
                    infoRetrieverDisposed = true;
                }
                if (!assetRetrieverDisposed)
                {
                    assetRetriever.Dispose();
                    assetRetrieverDisposed = true;
                }
                LoadNFT(model.Urn);
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

        internal async void LoadNFT(string urn)
        {
            try
            {
                infoRetrieverDisposed = false;
                NFTUtils.TryParseUrn(urn, out string contractAddress, out string tokenId);
                NFTInfo? info = await infoRetriever.FetchNFTInfoAsync(contractAddress, tokenId);

                if (!info.HasValue)
                {
                    LoadFailed();
                    return;
                }

                assetRetrieverDisposed = false;
                NFTInfo nftInfo = info.Value;
                INFTAsset nftAsset = await assetRetriever.LoadNFTAsset(nftInfo.previewImageUrl);

                if (nftAsset == null)
                {
                    LoadFailed();
                    return;
                }

                shapeFrame.SetImage(nftInfo.name, nftInfo.imageUrl, nftAsset);
                nftLoadedScr = urn;
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
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
