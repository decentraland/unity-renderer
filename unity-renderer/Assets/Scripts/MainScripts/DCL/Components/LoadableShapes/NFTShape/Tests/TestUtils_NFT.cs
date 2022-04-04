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
        INFTInfoLoadHelper infoLoadHelper = Substitute.For<INFTInfoLoadHelper>();
        INFTAssetLoadHelper assetLoadHelper = Substitute.For<INFTAssetLoadHelper>();
        NFTInfo mockedNftInfo = NFTInfo.defaultNFTInfo;

        INFTAsset mockedNftAsset = Substitute.For<INFTAsset>();

        infoLoadHelper
            .When((x) => { x.FetchNFTInfo(Arg.Any<string>(), Arg.Any<string>()); })
            .Do((x) =>
            {
                infoLoadHelper.OnFetchInfoSuccess += Raise.Event<Action<NFTInfo>>(mockedNftInfo);
            });

        assetLoadHelper.LoadNFTAsset(Arg.Any<string>(),
            Arg.Any<Action<INFTAsset>>(),
            Arg.Any<Action<Exception>>()).Returns(x =>
        {
            x.Arg<Action<INFTAsset>>().Invoke(mockedNftAsset);
            return null;
        });

        factory.RegisterBuilder((int) CLASS_ID.NFT_SHAPE, () =>
        {
            return new NFTShape(infoLoadHelper, assetLoadHelper);
        });
    }
}