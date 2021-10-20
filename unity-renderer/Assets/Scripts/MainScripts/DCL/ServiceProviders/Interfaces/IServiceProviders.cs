using System;
using DCL.Helpers.NFT.Markets;

public interface IServiceProviders : IDisposable
{
    ITheGraph theGraph { get; }
    ICatalyst catalyst { get; }
    IAnalytics analytics { get; }
    INFTMarket openSea { get; }
}