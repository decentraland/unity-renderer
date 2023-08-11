using DCL;

namespace DCLServices.EnvironmentProvider
{
    public interface IEnvironmentProviderService : IService
    {
        public bool IsProd();
    }
}
