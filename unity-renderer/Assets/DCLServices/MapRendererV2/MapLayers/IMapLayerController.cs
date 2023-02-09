using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCLServices.MapRendererV2.MapLayers
{
    public interface IMapLayerController : IDisposable
    {
        UniTask Enable(CancellationToken cancellationToken);

        UniTask Disable(CancellationToken cancellationToken);
    }
}
