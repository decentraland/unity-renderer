using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using NSubstitute;
using NFTShape_Internal;

// NOTE: NFTShapes meshes are rotated, so forward is actually backwards :S

public class NFTShapeHQImageHandlerShould
{
    private INFTAsset asset;
    private GameObject nftGO;
    private NFTShapeHQImageHandler imageHandler;
    private NFTShapeConfig nftShapeConfig;
    private bool isHQAsset;
    private Camera camera;

    [SetUp]
    protected void SetUp()
    {
        isHQAsset = false;
        camera = new GameObject("Camera"){tag = "MainCamera"}.AddComponent<Camera>();

        asset = Substitute.For<INFTAsset>();
        asset.When(a => a.RestorePreviewAsset()).Do(i =>
        {
            isHQAsset = false;
            asset.isHQ.Returns(false);
        });
        asset.WhenForAnyArgs(a => a.FetchAndSetHQAsset(null,null,null))
            .Do(i =>
            {
                isHQAsset = true;
                asset.isHQ.Returns(true);
            });

        nftGO = new GameObject();
        var nftController = NFTShapeFactory.InstantiateLoaderController(0).GetComponent<NFTShapeLoaderController>();
        nftController.transform.SetParent(nftGO.transform);
        nftController.transform.ResetLocalTRS();

        nftGO.transform.position = new Vector3(10,0,10);
        nftShapeConfig = nftController.config;

        var config = new NFTShapeHQImageConfig(){
            asset = asset,
            controller = nftController,
            nftConfig = nftShapeConfig
        };
        imageHandler = NFTShapeHQImageHandler.Create(config);
    }

    [TearDown]
    protected void TearDown()
    {
        Object.Destroy(nftGO);
        Object.Destroy(camera.gameObject);
    }

    [Test]
    public void SetHQImageCorrectly()
    {
        Assert.IsFalse(isHQAsset, "Shouldn't use HQ image");
        SetInFrontAndLookingToNFT();
        Assert.IsTrue(isHQAsset, "Should be using HQ image");
    }

    [Test]
    public void SetPreviewImageWhenCameraIsBehind()
    {
        SetInFrontAndLookingToNFT();
        Assert.IsTrue(isHQAsset, "Should be using HQ image");
        Camera.main.transform.position =
            nftGO.transform.position + nftGO.transform.forward * nftShapeConfig.hqImgMinDistance;
        imageHandler.Update();
        Assert.IsFalse(isHQAsset, "Shouldn't use HQ image");
    }

    [Test]
    public void SetPreviewImageWhenCameraLookAway()
    {
        SetInFrontAndLookingToNFT();
        Assert.IsTrue(isHQAsset, "Should be using HQ image");
        Camera.main.transform.forward = nftGO.transform.right;
        imageHandler.Update();
        Assert.IsFalse(isHQAsset, "Shouldn't use HQ image");
    }

    [Test]
    public void SetPreviewImageWhenPlayerMovesAway()
    {
        SetInFrontAndLookingToNFT();
        Assert.IsTrue(isHQAsset, "Should be using HQ image");
        CommonScriptableObjects.playerUnityPosition
            .Set(nftGO.transform.position - nftGO.transform.forward * (nftShapeConfig.hqImgMinDistance + 1));
        Assert.IsFalse(isHQAsset, "Shouldn't use HQ image");
    }

    void SetInFrontAndLookingToNFT()
    {
        CommonScriptableObjects.playerUnityPosition.Set(nftGO.transform.position - nftGO.transform.forward * nftShapeConfig.hqImgMinDistance);
        Camera.main.transform.position = CommonScriptableObjects.playerUnityPosition.Get();
        Camera.main.transform.LookAt(nftGO.transform);
        imageHandler.Update();
    }
}
