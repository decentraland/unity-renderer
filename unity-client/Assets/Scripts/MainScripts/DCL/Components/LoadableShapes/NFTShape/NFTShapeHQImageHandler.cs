using System;
using DCL;
using DCL.Helpers.NFT;
using UnityEngine;

public class NFTShapeHQImageConfig
{
    public string contentType;
    public NFTInfo nftInfo;
    public NFTShapeConfig nftConfig;
    public NFTShapeLoaderController controller;
    public AssetPromise_Texture previewTexture;
}

public class NFTShapeHQImageHandler : IDisposable
{
    NFTShapeHQImageConfig config;
    AssetPromise_Texture hqTexture;

    bool isPlayerNear;
    bool isCameraInFront;
    bool isPlayerLooking;

    Camera camera;
    Vector2 screenCenter;
    RaycastHit raycastHitInfo;

    static public NFTShapeHQImageHandler Create(NFTShapeHQImageConfig config)
    {
        if (config.contentType == "image/gif")
            return null;

        return new NFTShapeHQImageHandler(config);
    }

    public void Dispose()
    {
        CommonScriptableObjects.playerUnityPosition.OnChange -= OnPlayerPositionChanged;

        if (hqTexture != null)
        {
            ForgetHQTexture();
        }
    }

    public void Update()
    {
        if (config.controller.collider == null)
            return;

        if (!isPlayerNear)
            return;

        isCameraInFront = Vector3.Dot(config.controller.transform.forward, config.controller.transform.position - camera.transform.position)
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

        isPlayerLooking = Vector3.Dot(config.controller.transform.forward, camera.transform.forward) >= config.nftConfig.hqImgFacingDotProdMinValue;

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
        camera = Camera.main;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        CommonScriptableObjects.playerUnityPosition.OnChange += OnPlayerPositionChanged;
        OnPlayerPositionChanged(CommonScriptableObjects.playerUnityPosition, Vector3.zero);
    }

    private void OnPlayerPositionChanged(Vector3 current, Vector3 prev)
    {
        isPlayerNear = false;
        isCameraInFront = false;

        if (config.controller.collider == null)
            return;

        isPlayerNear = ((current - config.controller.collider.ClosestPointOnBounds(current)).sqrMagnitude
            < (config.nftConfig.hqImgMinDistance * config.nftConfig.hqImgMinDistance));

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
        if (hqTexture != null)
            return;

        hqTexture = new AssetPromise_Texture(string.Format("{0}=s{1}", config.nftInfo.imageUrl, config.nftConfig.hqImgResolution));
        AssetPromiseKeeper_Texture.i.Keep(hqTexture);
        hqTexture.OnSuccessEvent += (asset) => config.controller.UpdateTexture(asset.texture);
        hqTexture.OnFailEvent += (asset) => hqTexture = null;
        if (config.nftConfig.verbose)
        {
            Debug.Log($"Fetch {config.nftInfo.name} HQ image");
        }
    }

    private void ForgetHQTexture()
    {
        AssetPromiseKeeper_Texture.i.Forget(hqTexture);
        hqTexture = null;
        if (config.nftConfig.verbose)
        {
            Debug.Log($"Forget {config.nftInfo.name} HQ image");
        }
    }

    private void RestorePreviewTexture()
    {
        config.controller.UpdateTexture(config.previewTexture.asset.texture);
        if (config.nftConfig.verbose)
        {
            Debug.Log($"Restore {config.nftInfo.name} preview image");
        }
    }

    private void RestorePreviewTextureIfInHQ()
    {
        if (hqTexture == null)
            return;

        ForgetHQTexture();
        RestorePreviewTexture();
    }
}
