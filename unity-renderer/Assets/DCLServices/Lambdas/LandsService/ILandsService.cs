using System.Threading;

namespace DCLServices.Lambdas.LandsService
{
    public interface ILandsService
    {
        LambdaResponsePagePointer<LandsResponse> GetPaginationPointer(string address, int pageSize, CancellationToken ct);
    }
}
