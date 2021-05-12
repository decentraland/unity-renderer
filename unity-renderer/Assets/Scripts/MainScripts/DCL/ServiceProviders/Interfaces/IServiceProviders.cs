using System;

public interface IServiceProviders : IDisposable
{
    ITheGraph theGraph { get; }
    ICatalyst catalyst { get; }
}