using System;

public interface IServiceProviders : IDisposable
{
    ITheGraph theGraph { get; }
    ICatalyst catalyst { get; }
}

public class ServiceProviders : IServiceProviders
{
    public ITheGraph theGraph { get ; } = new TheGraph();
    public ICatalyst catalyst { get ; } = new Catalyst();

    public void Dispose()
    {
        catalyst.Dispose();
    }
}