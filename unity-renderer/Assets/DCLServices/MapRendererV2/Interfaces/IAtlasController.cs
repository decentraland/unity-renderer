using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public interface IAtlasController : IDisposable
{
    UniTask Initialize(CancellationToken ct);
}
