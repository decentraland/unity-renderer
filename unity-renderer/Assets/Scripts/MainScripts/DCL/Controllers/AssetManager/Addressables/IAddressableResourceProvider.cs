using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    public interface IAddressableResourceProvider : IService
    {
        UniTask<IList<T>> GetAddressablesList<T>(string key, CancellationToken cancellationToken = default);

        UniTask<T> GetAddressable<T>(string key, CancellationToken cancellationToken = default);

        UniTask<T> Instantiate<T>(string address, string name = "", Transform parent = null, CancellationToken cancellationToken = default);
    }
}
