using System;

public interface IServiceProviders : IDisposable
{
    ITheGraph theGraph { get; }
    ICatalyst catalyst { get; }
    IAnalytics analytics { get; }
}

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