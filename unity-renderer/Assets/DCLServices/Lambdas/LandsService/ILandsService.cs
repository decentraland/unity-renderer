using DCL;
using System.Threading;

namespace DCLServices.Lambdas.LandsService
{
    public interface ILandsService : IService
    {
        LambdaResponsePagePointer<LandsResponse> GetPaginationPointer(string address, int pageSize, CancellationToken ct);
    }
}
