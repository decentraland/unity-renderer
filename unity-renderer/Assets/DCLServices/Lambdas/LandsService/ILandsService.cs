using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.Lambdas.LandsService
{
    public interface ILandsService : IService
    {
        UniTask<(IReadOnlyList<LandsResponse.LandEntry> lands, int totalAmount)> RequestOwnedLandsAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct);
    }
}
