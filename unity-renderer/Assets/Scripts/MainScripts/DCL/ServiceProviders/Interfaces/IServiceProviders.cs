using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;

namespace DCL
{
    public interface IServiceProviders : IService
    {
        ITheGraph theGraph { get; }
        ICatalyst catalyst { get; }
        IAnalytics analytics { get; }
        IOpenSea openSea { get; }
    }
}