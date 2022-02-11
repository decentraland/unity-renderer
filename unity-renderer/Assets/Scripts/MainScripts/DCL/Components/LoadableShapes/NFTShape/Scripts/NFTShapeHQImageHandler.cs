using System;
using DCL.Helpers.NFT;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTShapeHQImageConfig
    {
        public string name;
        public string imageUrl;
        public NFTShapeConfig config;
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

            isCameraInFront = camera == null ||
                              Vector3.Dot(nftControllerT.forward,
                                  nftControllerT.position - camera.transform.position)
                              > config.config.hqImgInFrontDotProdMinValue;

            if (config.config.verbose)
            {
                Debug.Log($"Camera is in front of {config.name}? {isCameraInFront}");
            }

            if (!isCameraInFront)
            {
                RestorePreviewTextureIfInHQ();
                return;
            }

            isPlayerLooking = camera == null ||
                              Vector3.Dot(nftControllerT.forward, camera.transform.forward) >=
                              config.config.hqImgFacingDotProdMinValue;

            if (config.config.verbose)
            {
                Debug.Log($"Player is looking at {config.name}? {isPlayerLooking}");
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
                            <= (config.config.hqImgMinDistance * config.config.hqImgMinDistance));

            if (!isPlayerNear)
            {
                RestorePreviewTextureIfInHQ();
            }

            if (config.config.verbose)
            {
                Debug.Log($"Player position relative to {config.name} is near? {isPlayerNear}");
            }
        }

        private void FetchHQTexture()
        {
            if (asset.isHQ)
                return;

            string url = $"{config.imageUrl}=s{asset.hqResolution}";

            Action debugSuccess = null;
            Action<Exception> debugFail = null;

            if (config.config.verbose)
            {
                debugSuccess = () => Debug.Log($"Success: Fetch {config.name} HQ image");
                debugFail = error => Debug.Log($"Fail: Fetch {config.name} HQ image, Exception: {error}");
            }

            asset.FetchAndSetHQAsset(url, debugSuccess, debugFail);

            if (config.config.verbose)
            {
                Debug.Log($"Fetch {config.name} HQ image");
            }
        }

        private void RestorePreviewTexture()
        {
            asset.RestorePreviewAsset();
            if (config.config.verbose)
            {
                Debug.Log($"Restore {config.name} preview image");
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