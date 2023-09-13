using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.CustomNftCollection
{
    public interface ICustomNftCollectionService : IService
    {
        UniTask<string[]> GetConfiguredCustomNftCollectionAsync(CancellationToken cancellationToken);
        UniTask<string[]> GetConfiguredCustomNftItemsAsync(CancellationToken cancellationToken);
    }
}
