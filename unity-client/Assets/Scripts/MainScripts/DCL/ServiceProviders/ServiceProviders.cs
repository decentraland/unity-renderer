public interface IServiceProviders
{
    ITheGraph theGraph { get; }
}

public class ServiceProviders : IServiceProviders
{
    public ITheGraph theGraph { get ; } = new TheGraph();
}