using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Providers
{
    public interface IAddressableResolver : IService
    {
        UniTask<IList<T>> GetAddressablesListByLabel<T>(string label, CancellationToken cancellationToken = default);

        void IService.Initialize() { }

        void IDisposable.Dispose() { }
    }
}
