using System;
using DCL;
using DCL.Components;
using DCL.Helpers.NFT;
using DCL.Models;
using NFTShape_Internal;
using NSubstitute;
using UnityEngine;
using Environment = DCL.Environment;

public static class TestUtils_NFT
{
    public static void RegisterMockedNFTShape(IRuntimeComponentFactory factory)
    {
        INFTInfoRetriever infoRetriever = Substitute.For<INFTInfoRetriever>();
        INFTAssetRetriever assetRetriever = Substitute.For<INFTAssetRetriever>();
        NFTInfo mockedNftInfo = NFTInfo.defaultNFTInfo;

        INFTAsset mockedNftAsset = Substitute.For<INFTAsset>();

        infoRetriever
            .When((x) => { x.FetchNFTInfo(Arg.Any<string>(), Arg.Any<string>()); })
            .Do((x) =>
            {
                infoRetriever.OnFetchInfoSuccess += Raise.Event<Action<NFTInfo>>(mockedNftInfo);
            });

        assetRetriever.LoadNFTAsset(Arg.Any<string>(),
            Arg.Any<Action<INFTAsset>>(),
            Arg.Any<Action<Exception>>()).Returns(x =>
        {
            x.Arg<Action<INFTAsset>>().Invoke(mockedNftAsset);
            return null;
        });

        factory.RegisterBuilder((int) CLASS_ID.NFT_SHAPE, () =>
        {
            return new NFTShape(infoRetriever, assetRetriever);
        });
    }
}