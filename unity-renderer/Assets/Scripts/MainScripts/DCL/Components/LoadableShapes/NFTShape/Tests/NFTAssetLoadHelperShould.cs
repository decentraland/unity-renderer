using System.Collections;
using DCL;
using DCL.Helpers.NFT;
using NFTShape_Internal;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;


public class NFTAssetLoadHelperShould : IntegrationTestSuite
{
    private NFTAssetLoadHelper_Mock loadHelper;

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
    }

    protected override IEnumerator SetUp()
    {
        loadHelper = new NFTAssetLoadHelper_Mock();
        yield return base.SetUp();
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        loadHelper.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator LoadNftAsImage()
    {
        bool success = false;
        INFTAsset resultAsset = null;

        loadHelper.contentLengthToReturn = 10;
        loadHelper.contentTypeToReturn = "image/png";

        yield return loadHelper.LoadNFTAsset("fake_url", (x) =>
        {
            success = true;
            resultAsset = x;
        }, (x) => { });

        Assert.That(success, Is.True);
        Assert.That(resultAsset is NFTAsset_Image, Is.True);
    }

    [UnityTest]
    public IEnumerator LoadNftAsGif()
    {
        bool success = false;
        INFTAsset resultAsset = null;

        loadHelper.contentLengthToReturn = 10;
        loadHelper.contentTypeToReturn = "image/gif";

        yield return loadHelper.LoadNFTAsset("fake_url", (x) =>
        {
            success = true;
            resultAsset = x;
        }, (x) => { });

        Assert.That(success, Is.True);
        Assert.That(resultAsset is NFTAsset_Gif, Is.True);
    }
    
    [UnityTest]
    public IEnumerator FailWhenImageIsTooBig()
    {
        bool success = false;

        loadHelper.contentLengthToReturn = 10;
        loadHelper.contentTypeToReturn = "image/png";

        yield return loadHelper.LoadNFTAsset("fake_url_1", (x) => success = true, (x) => { });

        Assert.That(success, Is.True);

        loadHelper.contentLengthToReturn = 1000000;
        loadHelper.contentTypeToReturn = "image/png";

        success = false;
        yield return loadHelper.LoadNFTAsset("fake_url_2", (x) => success = true, (x) => { });

        Assert.That(success, Is.False);

        loadHelper.contentLengthToReturn = 1000000;
        loadHelper.contentTypeToReturn = "image/gif";

        success = false;
        yield return loadHelper.LoadNFTAsset("fake_url_3", (x) => success = true, (x) => { });

        Assert.That(success, Is.True);
    }

    [UnityTest]
    public IEnumerator UnloadImagesWhenDisposed()
    {
        bool success = false;
        yield return loadHelper.LoadNFTAsset("fake_url_1", (x) => success = true, (x) => Debug.Log(x));

        Assert.That(success, Is.True);
        Assert.That(loadHelper.hasLoadedAsset, Is.True);

        loadHelper.Dispose();

        Assert.That(loadHelper.hasLoadedAsset, Is.False);
    }
}