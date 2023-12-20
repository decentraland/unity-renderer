using DCL;
using MainScripts.DCL.ServiceProviders.OpenSea;
using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;

public class ServiceProviders : IServiceProviders
{
    public IOpenSea openSea { get; } = new OpenSeaService();
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
