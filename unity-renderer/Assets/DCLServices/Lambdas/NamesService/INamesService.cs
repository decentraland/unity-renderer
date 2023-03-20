using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.Lambdas.NamesService
{
    public interface INamesService : IService
    {
        UniTask<(IReadOnlyList<NamesResponse.NameEntry> names, int totalAmount)> RequestOwnedNamesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct);
    }
}
