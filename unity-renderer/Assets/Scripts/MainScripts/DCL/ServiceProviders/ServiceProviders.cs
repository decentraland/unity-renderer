using System;
using DCL;
using DCL.Helpers.NFT.Markets;

public class ServiceProviders : IServiceProviders
{
    public INFTMarket openSea { get; } = new OpenSea();
    public ITheGraph theGraph { get; } = new TheGraph();
    public ICatalyst catalyst { get; } = new Catalyst();
    public IAnalytics analytics { get; } = new Analytics();

    public void Dispose()
    {
        catalyst.Dispose();
        analytics.Dispose();
        theGraph.Dispose();
        openSea.Dispose();
    }

    public void Initialize()
    {
    }
}