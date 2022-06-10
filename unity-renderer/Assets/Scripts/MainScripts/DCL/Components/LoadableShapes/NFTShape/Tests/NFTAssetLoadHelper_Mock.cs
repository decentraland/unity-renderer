using System;
using System.Collections;
using System.Collections.Generic;
using DCL;

internal class NftAssetRetrieverMock : NFTAssetRetriever
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