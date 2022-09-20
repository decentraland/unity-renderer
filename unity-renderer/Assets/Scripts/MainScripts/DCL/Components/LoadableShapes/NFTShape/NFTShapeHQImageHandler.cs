using System;
using DCL;
using UnityEngine;

namespace NFTShape_Internal
{
    public class NFTShapeHQImageConfig
    {
        public string name;
        public string imageUrl;
        public Transform transform;
        public Collider collider;
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
            if (config.asset == null || !config.collider || !config.transform)
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
            if (!hqImageConfig.collider)
                return;

            if (!isPlayerNear)
                return;

            var config = DataStore.i.Get<DataStore_NFTShape>();

            isCameraInFront = camera == null ||
                              Vector3.Dot(nftControllerT.forward,
                                  nftControllerT.position - camera.transform.position)
                              > config.hqImgInFrontDotProdMinValue;

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
                              config.hqImgFacingDotProdMinValue;

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
            nftControllerT = hqImageConfig.transform;

            CommonScriptableObjects.playerUnityPosition.OnChange += OnPlayerPositionChanged;
            OnPlayerPositionChanged(CommonScriptableObjects.playerUnityPosition, Vector3.zero);
        }

        private void OnPlayerPositionChanged(Vector3 current, Vector3 prev)
        {
            isPlayerNear = false;

            if (!hqImageConfig.transform || !hqImageConfig.collider)
                return;

            var config = DataStore.i.Get<DataStore_NFTShape>();

            isPlayerNear = ((current - hqImageConfig.collider.ClosestPoint(current)).sqrMagnitude
                            <= (config.hqImgMinDistance *
                                config.hqImgMinDistance));

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