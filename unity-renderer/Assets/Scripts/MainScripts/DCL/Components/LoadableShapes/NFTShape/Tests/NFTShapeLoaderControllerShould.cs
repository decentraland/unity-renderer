using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers.NFT;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NFTShapeLoaderControllerShould
{
    private NFTShapeLoaderController loader;

    [SetUp]
    public void SetUp() { loader = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("NFTShapeLoader_Classic")).GetComponent<NFTShapeLoaderController>(); }

    [UnityTest]
    public IEnumerator DisposeOldPromisesInFetchNFTImageCoroutine()
    {
        List<IPromiseLike_TextureAsset> currentPromises = new List<IPromiseLike_TextureAsset>();

        IEnumerator mockFetch(string url, Action<IPromiseLike_TextureAsset> OnSuccess)
        {
            IPromiseLike_TextureAsset newPromise = Substitute.For<IPromiseLike_TextureAsset>();

            // Asset being null will avoid creating a NFTShapeHQImageHandler
            newPromise.asset.ReturnsForAnyArgs(x => null);

            currentPromises.Add(newPromise);
            OnSuccess.Invoke(newPromise);
            yield break;
        }

        NFTInfo nftInfo = new NFTInfo() { previewImageUrl = "it_doesnt_matters" };
        loader.wrappedTextureHelper = Substitute.For<IWrappedTextureHelper>();
        loader.wrappedTextureHelper
              .Configure()
              .Fetch(Arg.Any<string>(), Arg.Any<Action<IPromiseLike_TextureAsset>>())
              .Returns(x => mockFetch(x.ArgAt<string>(0), x.ArgAt<Action<IPromiseLike_TextureAsset>>(1)));


        loader.transform.position = CommonScriptableObjects.playerUnityPosition;

        yield return loader.FetchNFTImageCoroutine(nftInfo);
        Assert.NotNull(loader.assetPromise);
        Assert.AreEqual(1, currentPromises.Count);
        Assert.AreEqual(currentPromises[0], loader.assetPromise);

        yield return loader.FetchNFTImageCoroutine(nftInfo);
        Assert.NotNull(loader.assetPromise);
        currentPromises[0].Received().Forget();
        Assert.AreEqual(2, currentPromises.Count);
        Assert.AreEqual(currentPromises[1], loader.assetPromise);
    }

    [TearDown]
    public void TearDown() { UnityEngine.Object.Destroy(loader.gameObject); }
}