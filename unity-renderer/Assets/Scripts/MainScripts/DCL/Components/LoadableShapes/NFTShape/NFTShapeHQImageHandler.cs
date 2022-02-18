using System;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTShapeHQImageConfig
    {
        public string name;
        public string imageUrl;
        public NFTShapeConfig nftShapeConfig;
        public NFTShapeLoaderController controller;
        public INFTAsset asset;
    }

    public class NFTShapeHQImageHandler : IDisposable
    {
        public static bool VERBOSE = false;

        readonly NFTShapeHQImageConfig hqImageConfig;
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
            if (hqImageConfig.controller.collider is null)
                return;

            if (!isPlayerNear)
                return;

            isCameraInFront = camera == null ||
                              Vector3.Dot(nftControllerT.forward,
                                  nftControllerT.position - camera.transform.position)
                              > hqImageConfig.nftShapeConfig.hqImgInFrontDotProdMinValue;

            if (VERBOSE)
            {
                Debug.Log($"Camera is in front of {hqImageConfig.name}? {isCameraInFront}");
            }

            if (!isCameraInFront)
            {
                RestorePreviewTextureIfInHQ();
                return;
            }

            isPlayerLooking = camera == null ||
                              Vector3.Dot(nftControllerT.forward, camera.transform.forward) >=
                              hqImageConfig.nftShapeConfig.hqImgFacingDotProdMinValue;

            if (VERBOSE)
            {
                Debug.Log($"Player is looking at {hqImageConfig.name}? {isPlayerLooking}");
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

        private NFTShapeHQImageHandler(NFTShapeHQImageConfig hqImageConfig)
        {
            this.hqImageConfig = hqImageConfig;
            this.asset = hqImageConfig.asset;

            camera = Camera.main;
            nftControllerT = hqImageConfig.controller.transform;

            CommonScriptableObjects.playerUnityPosition.OnChange += OnPlayerPositionChanged;
            OnPlayerPositionChanged(CommonScriptableObjects.playerUnityPosition, Vector3.zero);
        }

        private void OnPlayerPositionChanged(Vector3 current, Vector3 prev)
        {
            isPlayerNear = false;

            if (hqImageConfig.controller == null || hqImageConfig.controller.collider == null)
                return;

            isPlayerNear = ((current - hqImageConfig.controller.collider.ClosestPoint(current)).sqrMagnitude
                            <= (hqImageConfig.nftShapeConfig.hqImgMinDistance *
                                hqImageConfig.nftShapeConfig.hqImgMinDistance));

            if (!isPlayerNear)
            {
                RestorePreviewTextureIfInHQ();
            }

            if (VERBOSE)
            {
                Debug.Log($"Player position relative to {hqImageConfig.name} is near? {isPlayerNear}");
            }
        }

        private void FetchHQTexture()
        {
            if (asset.isHQ)
                return;

            Action debugSuccess = null;
            Action<Exception> debugFail = null;

            if (VERBOSE)
            {
                debugSuccess = () => Debug.Log($"Success: Fetch {hqImageConfig.name} HQ image");
                debugFail = error => Debug.Log($"Fail: Fetch {hqImageConfig.name} HQ image, Exception: {error}");
            }

            // TODO(Brian): Asset is not supposed to fetch. Move this fetching mechanism to this class or elsewhere.
            asset.FetchAndSetHQAsset(hqImageConfig.imageUrl, debugSuccess, debugFail);

            if (VERBOSE)
            {
                Debug.Log($"Fetch {hqImageConfig.name} HQ image");
            }
        }

        private void RestorePreviewTexture()
        {
            asset.RestorePreviewAsset();
            if (VERBOSE)
            {
                Debug.Log($"Restore {hqImageConfig.name} preview image");
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