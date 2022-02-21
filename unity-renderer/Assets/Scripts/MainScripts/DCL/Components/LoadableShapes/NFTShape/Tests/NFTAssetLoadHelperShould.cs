using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers.NFT;
using NFTShape_Internal;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

internal class NFTAssetLoadHelper_Mock : NFTAssetLoadHelper
{
    public long contentLengthToReturn = 100;
    public string contentTypeToReturn = "image/png";

    public bool hasLoadedAsset => gifPromise != null || imagePromise != null;

    protected override IEnumerator GetHeaders(string url, HashSet<string> headerField,
        Action<Dictionary<string, string>> OnSuccess, Action<string> OnFail)
    {
        var headers = new Dictionary<string, string>();
        headers.Add("Content-Type", contentTypeToReturn.ToString());
        headers.Add("Content-Length", contentLengthToReturn.ToString());
        OnSuccess?.Invoke(headers);
        yield break;
    }

    protected override IEnumerator FetchGif(string url, Action<AssetPromise_Gif> OnSuccess,
        Action<Exception> OnFail = null)
    {
        var asset = new Asset_Gif() {id = url};
        AssetPromiseKeeper_Gif.i.library.Add(asset);
        var promise = new AssetPromise_Gif(url);
        OnSuccess?.Invoke(promise);
        yield break;
    }

    protected override IEnumerator FetchImage(string url, Action<AssetPromise_Texture> OnSuccess,
        Action<Exception> OnFail = null)
    {
        var asset = new Asset_Texture() {id = url};
        AssetPromiseKeeper_Texture.i.library.Add(asset);
        var promise = new AssetPromise_Texture(url);
        OnSuccess?.Invoke(promise);
        yield break;
    }
}

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