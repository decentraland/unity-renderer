using System;
using DCL.Helpers.NFT.Markets;

namespace DCL
{
    public interface IServiceProviders : IService
    {
        ITheGraph theGraph { get; }
        ICatalyst catalyst { get; }
        IAnalytics analytics { get; }
        INFTMarket openSea { get; }
    }
}