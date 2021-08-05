using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

public class NFTInfoFectcherShould
{

    [Test]
    public void NFTInfoFetcherFail()
    {
        //Arrange
        NFTInfoFetcher fetcher = new NFTInfoFetcher();

        //Act - Assert
        fetcher.FetchNFTImage("testAddress", "testId", info =>  Assert.Fail(), Assert.Pass );
    }

    [Test]
    public void NFTInfoFetcherDipose()
    {
        //Arrange
        NFTInfoFetcher fetcher = new NFTInfoFetcher();
        fetcher.FetchNFTImage("testAddress", "testId", null, null );

        //Act
        fetcher.Dispose();

        //Assert
        Assert.IsNull(fetcher.fetchCoroutine);
    }
}