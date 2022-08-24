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
    private NftAssetRetrieverMock retriever;

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
    }

    protected override IEnumerator SetUp()
    {
        retriever = new NftAssetRetrieverMock();
        yield return base.SetUp();
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        retriever.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator LoadNftAsImage()
    {
        bool success = false;
        INFTAsset resultAsset = null;

        retriever.contentLengthToReturn = 10;
        retriever.contentTypeToReturn = "image/png";

        yield return retriever.LoadNFTAsset("fake_url", (x) =>
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

        retriever.contentLengthToReturn = 10;
        retriever.contentTypeToReturn = "image/gif";

        yield return retriever.LoadNFTAsset("fake_url", (x) =>
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

        retriever.contentLengthToReturn = 10;
        retriever.contentTypeToReturn = "image/png";

        yield return retriever.LoadNFTAsset("fake_url_1", (x) => success = true, (x) => { });

        Assert.That(success, Is.True);

        retriever.contentLengthToReturn = 1000000;
        retriever.contentTypeToReturn = "image/png";

        success = false;
        yield return retriever.LoadNFTAsset("fake_url_2", (x) => success = true, (x) => { });

        Assert.That(success, Is.False);

        retriever.contentLengthToReturn = 1000000;
        retriever.contentTypeToReturn = "image/gif";

        success = false;
        yield return retriever.LoadNFTAsset("fake_url_3", (x) => success = true, (x) => { });

        Assert.That(success, Is.True);
    }

    [UnityTest]
    public IEnumerator UnloadImagesWhenDisposed()
    {
        bool success = false;
        yield return retriever.LoadNFTAsset("fake_url_1", (x) => success = true, (x) => Debug.Log(x));

        Assert.That(success, Is.True);
        Assert.That(retriever.hasLoadedAsset, Is.True);

        retriever.Dispose();

        Assert.That(retriever.hasLoadedAsset, Is.False);
    }
}