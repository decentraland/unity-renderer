using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.CustomNftCollection
{
    public interface ICustomNftCollectionService : IService
    {
        UniTask<IReadOnlyList<string>> GetParametrizedCustomNftCollectionAsync(CancellationToken cancellationToken);
    }
}
