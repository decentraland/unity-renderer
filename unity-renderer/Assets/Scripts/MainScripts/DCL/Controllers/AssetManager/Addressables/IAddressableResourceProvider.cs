using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Providers
{
    public interface IAddressableResourceProvider<T>
    {
        UniTask<IList<T>> GetAddressablesList(string key, CancellationToken cancellationToken = default);

        UniTask<T> GetAddressable(string key, CancellationToken cancellationToken = default);
    }
}
