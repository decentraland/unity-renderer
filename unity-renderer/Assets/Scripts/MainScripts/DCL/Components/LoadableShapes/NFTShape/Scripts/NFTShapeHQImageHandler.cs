using System;
using DCL.Helpers.NFT;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTShapeHQImageConfig
    {
        public NFTInfo nftInfo;
        public NFTShapeConfig nftConfig;
        public NFTShapeLoaderController controller;
        public INFTAsset asset;
    }

    public class NFTShapeHQImageHandler : IDisposable
    {
        readonly NFTShapeHQImageConfig config;
        readonly INFTAsset asset;
        readonly Camera camera;
        readonly Transform nftControllerT;

        bool isPlayerNear;
        bool isCameraInFront;
        bool isPlayerLooking;

        public static NFTShapeHQImageHandler Create(NFTShapeHQImageConfig config)
        {
            if (config.asset == null || config.controller == null)
            {
                return null;
            }

            return new NFTShapeHQImageHandler(config);
        }

        public void Dispose()
        {
            CommonScriptableObjects.playerUnityPosition.OnChange -= OnPlayerPositionChanged;
            asset.Dispose();
        }

        public void Update()
        {
            if (config.controller.collider is null)
                return;

            if (!isPlayerNear)
                return;

            isCameraInFront = Vector3.Dot(nftControllerT.forward,
                                  nftControllerT.position - camera.transform.position)
                              > config.nftConfig.hqImgInFrontDotProdMinValue;

            if (config.nftConfig.verbose)
            {
                Debug.Log($"Camera is in front of {config.nftInfo.name}? {isCameraInFront}");
            }

            if (!isCameraInFront)
            {
                RestorePreviewTextureIfInHQ();
                return;
            }

            isPlayerLooking = Vector3.Dot(nftControllerT.forward, camera.transform.forward) >=
                              config.nftConfig.hqImgFacingDotProdMinValue;

            if (config.nftConfig.verbose)
            {
                Debug.Log($"Player is looking at {config.nftInfo.name}? {isPlayerLooking}");
            }

            if (isPlayerLooking)
            {
                FetchHQTexture();
            }
            else
            {
                RestorePreviewTextureIfInHQ();
            }
        }

        private NFTShapeHQImageHandler(NFTShapeHQImageConfig config)
        {
            this.config = config;
            this.asset = config.asset;

            camera = Camera.main;
            nftControllerT = config.controller.transform;

            CommonScriptableObjects.playerUnityPosition.OnChange += OnPlayerPositionChanged;
            OnPlayerPositionChanged(CommonScriptableObjects.playerUnityPosition, Vector3.zero);
        }

        private void OnPlayerPositionChanged(Vector3 current, Vector3 prev)
        {
            isPlayerNear = false;

            if (config.controller == null || config.controller.collider == null)
                return;

            isPlayerNear = ((current - config.controller.collider.ClosestPoint(current)).sqrMagnitude
                            <= (config.nftConfig.hqImgMinDistance * config.nftConfig.hqImgMinDistance));

            if (!isPlayerNear)
            {
                RestorePreviewTextureIfInHQ();
            }

            if (config.nftConfig.verbose)
            {
                Debug.Log($"Player position relative to {config.nftInfo.name} is near? {isPlayerNear}");
            }
        }

        private void FetchHQTexture()
        {
            if (asset.isHQ)
                return;

            string url = $"{config.nftInfo.imageUrl}=s{asset.hqResolution}";

            Action debugSuccess = null;
            Action debugFail = null;

            if (config.nftConfig.verbose)
            {
                debugSuccess = () => Debug.Log($"Success: Fetch {config.nftInfo.name} HQ image");
                debugFail = () => Debug.Log($"Fail: Fetch {config.nftInfo.name} HQ image");
            }

            asset.FetchAndSetHQAsset(url, debugSuccess, debugFail);

            if (config.nftConfig.verbose)
            {
                Debug.Log($"Fetch {config.nftInfo.name} HQ image");
            }
        }

        private void RestorePreviewTexture()
        {
            asset.RestorePreviewAsset();
            if (config.nftConfig.verbose)
            {
                Debug.Log($"Restore {config.nftInfo.name} preview image");
            }
        }

        private void RestorePreviewTextureIfInHQ()
        {
            if (!asset.isHQ)
                return;

            RestorePreviewTexture();
        }
    }
}