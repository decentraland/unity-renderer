using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL
{
    public interface IService : IDisposable
    {
        void Initialize();
        UniTask InitializeAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}
