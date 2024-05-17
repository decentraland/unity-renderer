using DCL;
using DCL.Components;
using DCL.Models;
using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;
using NFTShape_Internal;
using NSubstitute;
using System;

public static class TestUtils_NFT
{
    public static void RegisterMockedNFTShape(IRuntimeComponentFactory factory)
    {
        INFTInfoRetriever infoRetriever = Substitute.For<INFTInfoRetriever>();
        INFTAssetRetriever assetRetriever = Substitute.For<INFTAssetRetriever>();
        NFTInfo mockedNftInfo = NFTInfo.Default;

        INFTAsset mockedNftAsset = Substitute.For<INFTAsset>();

        infoRetriever
            .When((x) => { x.FetchNFTInfo(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()); })
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