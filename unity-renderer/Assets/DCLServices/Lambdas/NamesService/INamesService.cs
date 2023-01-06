using DCL;
using System.Threading;

namespace DCLServices.Lambdas.NamesService
{
    public interface INamesService : IService
    {
        LambdaResponsePagePointer<NamesResponse> GetPaginationPointer(string address, int pageSize, CancellationToken ct);
    }
}
