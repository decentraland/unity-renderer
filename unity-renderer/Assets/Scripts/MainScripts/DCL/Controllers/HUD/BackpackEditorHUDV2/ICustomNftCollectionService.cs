using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Backpack
{
    public interface ICustomNftCollectionService : IService
    {
        UniTask<IReadOnlyList<string>> GetConfiguredCustomNftCollectionAsync(CancellationToken cancellationToken);
        UniTask<IReadOnlyList<string>> GetConfiguredCustomNftItemsAsync(CancellationToken cancellationToken);
    }
}
