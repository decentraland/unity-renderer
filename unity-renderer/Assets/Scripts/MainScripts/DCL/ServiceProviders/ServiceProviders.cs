using System;

public class ServiceProviders : IServiceProviders
{
    public ITheGraph theGraph { get ; } = new TheGraph();
    public ICatalyst catalyst { get ; } = new Catalyst();
    public IAnalytics analytics { get ; } = new Analytics();

    public void Dispose()
    {
        catalyst.Dispose();
        analytics.Dispose();
    }
}