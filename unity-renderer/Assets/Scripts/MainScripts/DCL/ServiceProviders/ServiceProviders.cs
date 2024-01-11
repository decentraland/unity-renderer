using DCL;
using MainScripts.DCL.ServiceProviders.OpenSea;
using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;

public class ServiceProviders : IServiceProviders
{
    private readonly KernelConfig kernelConfig;

    public ServiceProviders(KernelConfig kernelConfig)
    {
        this.kernelConfig = kernelConfig;
        openSea = new OpenSeaService(this.kernelConfig);
    }

    public IOpenSea openSea { get; }
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
